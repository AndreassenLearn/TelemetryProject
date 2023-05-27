using TelemetryProject.Common.Models;

namespace TelemetryProject.CommonClient;

public interface ISignalRClientService
{
    public delegate void LiveHumidexDelegate(Humidex humidex);

    /// <summary>
    /// Start listening for new humidex measurements.
    /// </summary>
    /// <param name="newHumidexDelegate">Delegate method to process the new humidex.</param>
    public void StartLiveHumidex(LiveHumidexDelegate newHumidexDelegate);

    /// <summary>
    /// Stop listening for new humidex measurements.
    /// </summary>
    public void StopLiveHumidex();
}
