using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Fcs.Campaign.Application.Abstractions.Authentication;
using Fcs.Campaign.WebApi.Authentication;
using Fcs.Campaign.WebApi.Middlewares;
using Fcs.Campaign.WebApi.Observability;
using Fcs.Campaign.WebApi.Settings;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Fcs.Campaign.WebApi.DependencyInjection;

[ExcludeFromCodeCoverage]

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "fcs.Campaign API",
                Version = "v1"
            });
        });

        services.AddCorsConfiguration(configuration);
        services.AddHealthChecks();
        services.AddRouting(options => options.LowercaseUrls = true);

        services.AddObservability(configuration);
        services.AddSerilogLogging(configuration);

        return services;
    }

    public static WebApplication UseWebApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health", new HealthCheckOptions());
        app.MapPrometheusScrapingEndpoint("/metrics");
        return app;
    }

    private static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(CorsSettings.SectionName)
            .Get<CorsSettings>()
            ?? new CorsSettings { AllowedOrigins = ["http://localhost:4200", "http://127.0.0.1:4200"] };

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(settings.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    private static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new ObservabilityOptions();
        configuration.GetSection(ObservabilityOptions.SectionName).Bind(options);

        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(options.ServiceName, serviceNamespace: "FCS")
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environment
            });

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName, serviceNamespace: "FCS"))
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.Filter = httpContext =>
                            !httpContext.Request.Path.StartsWithSegments("/health") &&
                            !httpContext.Request.Path.StartsWithSegments("/metrics");
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();

                if (options.EnableOtlpExporter && !string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    builder.AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri($"{options.OtlpEndpoint}/v1/traces");
                        exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                        if (!string.IsNullOrWhiteSpace(options.OtlpAuthHeader))
                        {
                            exporterOptions.Headers = $"Authorization={options.OtlpAuthHeader}";
                        }
                    });
                }
            })
            .WithMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();

                if (options.EnableOtlpExporter && !string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    builder.AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri($"{options.OtlpEndpoint}/v1/metrics");
                        exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                        if (!string.IsNullOrWhiteSpace(options.OtlpAuthHeader))
                        {
                            exporterOptions.Headers = $"Authorization={options.OtlpAuthHeader}";
                        }
                    });
                }
            });

        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new ObservabilityOptions();
        configuration.GetSection(ObservabilityOptions.SectionName).Bind(options);
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", options.ServiceName)
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console();

        if (options.EnableOtlpExporter && !string.IsNullOrWhiteSpace(options.OtlpEndpoint))
        {
            loggerConfiguration.WriteTo.OpenTelemetry(otlpOptions =>
            {
                otlpOptions.Endpoint = $"{options.OtlpEndpoint}/v1/logs";
                otlpOptions.Protocol = OtlpProtocol.HttpProtobuf;
                if (!string.IsNullOrWhiteSpace(options.OtlpAuthHeader))
                {
                    otlpOptions.Headers = new Dictionary<string, string>
                    {
                        ["Authorization"] = options.OtlpAuthHeader
                    };
                }
                otlpOptions.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = options.ServiceName,
                    ["service.namespace"] = "FCS",
                    ["deployment.environment"] = environment
                };
            });
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        return services;
    }
}
