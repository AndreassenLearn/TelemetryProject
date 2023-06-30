#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TelemetryProject.BlazorClient/TelemetryProject.BlazorClient.csproj", "TelemetryProject.BlazorClient/"]
COPY ["TelemetryProject.CommonClient/TelemetryProject.CommonClient.csproj", "TelemetryProject.CommonClient/"]
COPY ["TelemetryProject.Common/TelemetryProject.Common.csproj", "TelemetryProject.Common/"]
RUN dotnet restore "TelemetryProject.BlazorClient/TelemetryProject.BlazorClient.csproj"
COPY . .
WORKDIR "/src/TelemetryProject.BlazorClient"
RUN dotnet build "TelemetryProject.BlazorClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelemetryProject.BlazorClient.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelemetryProject.BlazorClient.dll"]