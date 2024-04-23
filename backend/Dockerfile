# Base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build environment and build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy the main project and its dependencies
COPY ["NetBackend/NetBackend.csproj", "NetBackend/"]
COPY ["NetBackend.Tests/NetBackend.Tests.csproj", "NetBackend.Tests/"]
# Restore packages for both projects
RUN dotnet restore "NetBackend/NetBackend.csproj"
RUN dotnet restore "NetBackend.Tests/NetBackend.Tests.csproj"
# Copy the rest of the source code
COPY . .
# Build the main project
RUN dotnet build "NetBackend/NetBackend.csproj" -c Release -o /app/build
# Test the main project
RUN dotnet test "NetBackend.Tests/NetBackend.Tests.csproj" --verbosity normal

# Publish the application
FROM build AS publish
RUN dotnet publish "NetBackend/NetBackend.csproj" -c Release -o /app/publish

# Build the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetBackend.dll"]
