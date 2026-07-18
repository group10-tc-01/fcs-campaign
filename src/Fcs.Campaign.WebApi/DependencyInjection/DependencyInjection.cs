using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Fcs.Campaign.Application.Abstractions.Authentication;
using Fcs.Campaign.WebApi.Filters;
using Fcs.Campaign.WebApi.Observability;
using Fcs.Campaign.WebApi.Settings;
using Fcs.Campaign.WebApi.Swagger;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Fcs.Campaign.WebApi.DependencyInjection;

[ExcludeFromCodeCoverage]

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddCampaignSwagger();
        services.AddCorsConfiguration(configuration);
        services.AddVersioning();
        services.AddFilters();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddObservabilitySettings(configuration);
        services.AddObservability(configuration);
        services.AddSerilogLogging(configuration);

        return services;
    }

    private static void AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
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
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private static void AddVersioning(this IServiceCollection services)
    {
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
    }

    private static void AddFilters(this IServiceCollection services)
    {
        services.AddMvc(options =>
        {
            options.Filters.Add<TrimStringsActionFilter>();
        });
    }

    private static void AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = GetObservabilitySettings(configuration);
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var resourceBuilder = ObservabilityTelemetry.CreateResourceBuilder(settings, environment);

        services.AddOpenTelemetry()
            .WithTracing(builder => builder.ConfigureTracing(settings, resourceBuilder))
            .WithMetrics(builder => builder.ConfigureMetrics(settings, resourceBuilder));
    }

    private static void AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = GetObservabilitySettings(configuration);
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.With<TraceContextEnricher>()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("Application", "Fcs.Campaign")
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");

        if (settings.EnableOtlpExporter && !string.IsNullOrWhiteSpace(settings.OtlpEndpoint))
        {
            loggerConfiguration.WriteTo.OpenTelemetry(otlpOptions =>
            {
                otlpOptions.Endpoint = $"{settings.OtlpEndpoint}/v1/logs";
                otlpOptions.Protocol = OtlpProtocol.HttpProtobuf;
                if (!string.IsNullOrWhiteSpace(settings.OtlpAuthHeader))
                {
                    otlpOptions.Headers = new Dictionary<string, string>
                    {
                        ["Authorization"] = settings.OtlpAuthHeader
                    };
                }
                otlpOptions.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = settings.ServiceName,
                    ["deployment.environment"] = environment
                };
            });
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        Log.Information("Starting {Application} application", "Fcs.Campaign");
        Log.Information("Environment: {Environment}", environment);

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });
    }

    private static void AddObservabilitySettings(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ObservabilitySettings>()
            .Bind(configuration.GetRequiredSection(ObservabilitySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static ObservabilitySettings GetObservabilitySettings(IConfiguration configuration)
    {
        return configuration
            .GetRequiredSection(ObservabilitySettings.SectionName)
            .Get<ObservabilitySettings>()
            ?? throw new InvalidOperationException("Observability settings must be configured.");
    }
}
