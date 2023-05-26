using Common.Models;

namespace TelemetryProject.Services.SignalR
{
    public interface ISignalRService
    {
        /// <summary>
        /// Publish to <see cref="Common.Constants.SignalR.LiveHumidexMethodName"/>.
        /// </summary>
        /// <param name="humidex">Newly measured humidex.</param>
        /// <returns></returns>
        public Task PublishLiveHumidexAsync(Humidex humidex);
    }
}
