using MockSensors.Sensors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string sensorName = "MySensor"; // Replace this with your actual value
builder.Services.AddSingleton(sensorName);
builder.Services.AddSingleton<WaterQualitySensor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Retrieve the sensor instance from DI
var sensor = app.Services.GetRequiredService<WaterQualitySensor>();

app.MapGet("/startSensor", () =>
{
    sensor.Start(); // Start without creating a new instance
    return Results.Ok("Sensor started");
});

app.MapGet("/stopSensor", () =>
{
    sensor.Stop(); // Stop the sensor
    return Results.Ok("Sensor stopped");
});


// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
