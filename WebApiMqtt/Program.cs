using Microsoft.AspNetCore.Builder;
using Services;
using Services.MqttService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<MqttClientWorker>();
builder.Services.AddSingleton<IMqttClientPublish, MqttClientPublish>();
builder.Services.AddSingleton<IInfluxDbService, InfluxDbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/servo/{position}", async (ushort position, IMqttClientPublish publish) =>
{
    await publish.ServoAsync(position);
});

app.MapGet("/humidex", (IInfluxDbService influxDbService) =>
{
    return influxDbService.ReadAllHumidex();
});

app.MapGet("/humidex/{startTime}/{endTime}", (DateTime startTime, DateTime endTime, IInfluxDbService influxDbService) =>
{
    return influxDbService.ReadAllHumidex(startTime, endTime);
});

app.Run();
