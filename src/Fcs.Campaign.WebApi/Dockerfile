FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/Fcs.Campaign.Domain/Fcs.Campaign.Domain.csproj", "src/Fcs.Campaign.Domain/"]
COPY ["src/Fcs.Campaign.Messages/Fcs.Campaign.Messages.csproj", "src/Fcs.Campaign.Messages/"]
COPY ["src/Fcs.Campaign.Application/Fcs.Campaign.Application.csproj", "src/Fcs.Campaign.Application/"]
COPY ["src/Fcs.Campaign.Infrastructure.Auth/Fcs.Campaign.Infrastructure.Auth.csproj", "src/Fcs.Campaign.Infrastructure.Auth/"]
COPY ["src/Fcs.Campaign.Infrastructure.Kafka/Fcs.Campaign.Infrastructure.Kafka.csproj", "src/Fcs.Campaign.Infrastructure.Kafka/"]
COPY ["src/Fcs.Campaign.Infrastructure.SqlServer/Fcs.Campaign.Infrastructure.SqlServer.csproj", "src/Fcs.Campaign.Infrastructure.SqlServer/"]
COPY ["src/Fcs.Campaign.WebApi/Fcs.Campaign.WebApi.csproj", "src/Fcs.Campaign.WebApi/"]

RUN dotnet restore "./src/Fcs.Campaign.WebApi/Fcs.Campaign.WebApi.csproj"

COPY . .
WORKDIR "/src/src/Fcs.Campaign.WebApi"
RUN dotnet publish "./Fcs.Campaign.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fcs.Campaign.WebApi.dll"]
