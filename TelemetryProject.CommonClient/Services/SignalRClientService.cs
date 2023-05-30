using Microsoft.AspNetCore.SignalR.Client;
using TelemetryProject.Common.Models;
using static TelemetryProject.CommonClient.Services.ISignalRClientService;

namespace TelemetryProject.CommonClient.Services;

public class SignalRClientService : ISignalRClientService
{
    private readonly HubConnection _connection;

    public SignalRClientService()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(Constants.ApiBaseUrl + Common.Constants.SignalR.HumidexHubUri)
            .Build();

        Task.Run(async () =>
        {
            await _connection.StartAsync();
        });
    }

    /// <inheritdoc/>
    public void StartLiveHumidex(LiveHumidexDelegate newHumidexDelegate)
    {
        _connection.On<Humidex>(Common.Constants.SignalR.LiveHumidexMethodName, humidex =>
        {
            newHumidexDelegate(humidex);
        });
    }

    /// <inheritdoc/>
    public void StopLiveHumidex()
    {
        _connection.Remove(Common.Constants.SignalR.LiveHumidexMethodName);
    }
}
