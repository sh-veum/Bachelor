FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WaterQualitySensor.csproj", "./"]
RUN dotnet restore "WaterQualitySensor.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "WaterQualitySensor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WaterQualitySensor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WaterQualitySensor.dll"]