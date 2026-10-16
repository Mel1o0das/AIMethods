FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "Directory.Packages.props", "BookingService.sln", "./"]
COPY ["BookingService.Domain/BookingService.Domain.csproj", "BookingService.Domain/"]
COPY ["BookingService.Application/BookingService.Application.csproj", "BookingService.Application/"]
COPY ["BookingService.Infrastructure.Postgres/BookingService.Infrastructure.Postgres.csproj", "BookingService.Infrastructure.Postgres/"]
COPY ["BookingService.Core/BookingService.Core.csproj", "BookingService.Core/"]
COPY ["BookingService.Tests/BookingService.Tests.csproj", "BookingService.Tests/"]
RUN dotnet restore BookingService.sln

COPY . .
RUN dotnet publish BookingService.Core/BookingService.Core.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/images /app/DataProtection-Keys \
    && chown -R app:app /app/images /app/DataProtection-Keys
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "BookingService.Core.dll"]
