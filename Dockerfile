FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["VendorHub/VendorHub.csproj", "VendorHub/"]

RUN dotnet restore "VendorHub/VendorHub.csproj"

COPY ["VendorHub/", "VendorHub/"]

RUN dotnet build "VendorHub/VendorHub.csproj" -c Release -o /app/build

RUN dotnet publish "VendorHub/VendorHub.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "VendorHub.dll"]