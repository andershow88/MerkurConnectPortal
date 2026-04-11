FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MerkurConnectPortal.Web/MerkurConnectPortal.Web.csproj", "src/MerkurConnectPortal.Web/"]
COPY ["src/MerkurConnectPortal.Application/MerkurConnectPortal.Application.csproj", "src/MerkurConnectPortal.Application/"]
COPY ["src/MerkurConnectPortal.Domain/MerkurConnectPortal.Domain.csproj", "src/MerkurConnectPortal.Domain/"]
COPY ["src/MerkurConnectPortal.Infrastructure/MerkurConnectPortal.Infrastructure.csproj", "src/MerkurConnectPortal.Infrastructure/"]
RUN dotnet restore "src/MerkurConnectPortal.Web/MerkurConnectPortal.Web.csproj"
COPY . .
WORKDIR "/src/src/MerkurConnectPortal.Web"
RUN dotnet build "MerkurConnectPortal.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MerkurConnectPortal.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Uploads-Verzeichnis anlegen
RUN mkdir -p /app/wwwroot/uploads

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MerkurConnectPortal.Web.dll"]
