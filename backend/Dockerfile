FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["VendorHub.csproj", "./"]

RUN dotnet restore "VendorHub.csproj"

COPY [".", "./"]

RUN dotnet build "VendorHub.csproj" -c Release -o /app/build

RUN dotnet publish "VendorHub.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/wwwroot/Images/Categories /app/wwwroot/Images/Products

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "VendorHub.dll"]