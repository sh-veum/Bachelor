Requires:
  Docker Desktop
  .NET 8.0 SDK
run command: docker-compose up --build

To Test: http://localhost:8088/swagger
DB:
  Port: 5433
  User ID: postgres
  password: password
  Database: postgresTestDB

Might have to run command: dotnet ef migrations add Initial
  Remember to install the dotnet-ef tool
