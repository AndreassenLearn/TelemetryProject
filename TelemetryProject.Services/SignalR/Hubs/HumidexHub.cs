using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace TelemetryProject.Services.SignalR.Hubs
{
    public class HumidexHub : Hub
    {
        private readonly ILogger<HumidexHub> _logger;

        public HumidexHub(ILogger<HumidexHub> logger)
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null) _logger.LogWarning("Warning: {Message}", exception.Message);
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }
    }
}
