using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetBackend.Data;
using NetBackend.Models.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add authentication (modified from https://youtu.be/ORzt0lks2H4?si=kXqRl7VUO9r9BPbN)
builder.Services.AddAuthentication("Bearer")
    .AddBearerToken(IdentityConstants.BearerScheme);

// Add authorization
builder.Services.AddAuthorizationBuilder();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<ApiDbContext>()
    .AddApiEndpoints();

var app = builder.Build();

app.MapIdentityApi<User>();

// Automatic migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    dbContext.Database.Migrate();
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
