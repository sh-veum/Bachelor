using MockSensors.Sensors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WaterQualitySensorManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var sensorManager = app.Services.GetRequiredService<WaterQualitySensorManager>();

app.MapGet("/startSensor/{id}", (string id) =>
{
    if (sensorManager.TryStartSensor(id))
    {
        return Results.Ok($"Sensor {id} started");
    }
    else
    {
        return Results.BadRequest($"Sensor {id} is already running");
    }
});

app.MapGet("/stopSensor/{id}", (string id) =>
{
    if (sensorManager.TryStopSensor(id))
    {
        return Results.Ok($"Sensor {id} stopped");
    }
    else
    {
        return Results.BadRequest($"Sensor {id} was not found");
    }
});

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
