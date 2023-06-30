#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TelemetryProject.WebApi/TelemetryProject.WebApi.csproj", "TelemetryProject.WebApi/"]
COPY ["TelemetryProject.Common/TelemetryProject.Common.csproj", "TelemetryProject.Common/"]
COPY ["TelemetryProject.Services/TelemetryProject.Services.csproj", "TelemetryProject.Services/"]
RUN dotnet restore "TelemetryProject.WebApi/TelemetryProject.WebApi.csproj"
COPY . .
WORKDIR "/src/TelemetryProject.WebApi"
RUN dotnet build "TelemetryProject.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelemetryProject.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelemetryProject.WebApi.dll"]