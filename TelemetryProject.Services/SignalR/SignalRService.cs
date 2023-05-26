using Common.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TelemetryProject.Services.SignalR.Hubs;

namespace TelemetryProject.Services.SignalR
{
    public class SignalRService : ISignalRService
    {
        private readonly ILogger<SignalRService> _logger;
        private readonly IHubContext<HumidexHub> _humidexHubContext;

        public SignalRService(ILogger<SignalRService> logger, IHubContext<HumidexHub> humidexHubContext)
        {
            _logger = logger;
            _humidexHubContext = humidexHubContext;
        }

        /// <inheritdoc/>
        public async Task PublishLiveHumidexAsync(Humidex humidex)
        {
            await _humidexHubContext.Clients.All.SendAsync(Common.Constants.SignalR.LiveHumidexMethodName, humidex);
            _logger.LogInformation("Publish {Method}: {Parameter}.", Common.Constants.SignalR.LiveHumidexMethodName, humidex);
        }
    }
}
