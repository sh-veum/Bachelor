# Requires:

Docker Desktop

.NET 8.0 SDK

run command: "docker-compose up --build"


### To Test: http://localhost:8088/swagger

## DB:
  Port: 5433
  UserID: postgres
  password: password
  Database: postgresTestDB

## Error
Might have to run command: "dotnet ef migrations add Initial". If so, remember to install the dotnet-ef tool
