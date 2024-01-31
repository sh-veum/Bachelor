using backend.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NetBackend.Data;
using NetBackend.Models.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.MapType<DatabaseType>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(DatabaseType))
                   .Select(name => new OpenApiString(name) as IOpenApiAny)
                   .ToList()
    });
});

// Add authentication (modified from https://youtu.be/ORzt0lks2H4?si=kXqRl7VUO9r9BPbN)
builder.Services.AddAuthentication("Bearer")
    .AddBearerToken(IdentityConstants.BearerScheme);

// Add authorization
builder.Services.AddAuthorizationBuilder();

// Configure DbContext
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var customerOneConnectionString = builder.Configuration.GetConnectionString("CustomerOneConnection");
var customerTwoConnectionString = builder.Configuration.GetConnectionString("CustomerTwoConnection");

builder.Services.AddDbContext<MainDbContext>(options =>
    options.UseNpgsql(defaultConnectionString));

builder.Services.AddDbContext<CustomerOneDbContext>(options =>
    options.UseNpgsql(customerOneConnectionString));

builder.Services.AddDbContext<CustomerTwoDbContext>(options =>
    options.UseNpgsql(customerTwoConnectionString));

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<MainDbContext>()
    .AddApiEndpoints();

var app = builder.Build();

app.MapIdentityApi<User>();

// Automatic migration
using (var scope = app.Services.CreateScope())
{
    var mainDbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
    mainDbContext.Database.Migrate();

    var customerOneDbContext = scope.ServiceProvider.GetRequiredService<CustomerOneDbContext>();
    customerOneDbContext.Database.Migrate();

    var customerTwoDbContext = scope.ServiceProvider.GetRequiredService<CustomerTwoDbContext>();
    customerTwoDbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.¨
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
