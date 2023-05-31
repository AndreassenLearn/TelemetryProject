using TelemetryProject.Common.Models;

namespace TelemetryProject.CommonClient.Services;

public interface IHumidexService
{
    /// <summary>
    /// Get humidexes within a specific time interval.
    /// </summary>
    /// <param name="startTime">Start of interval.</param>
    /// <param name="endTime">End of interval.</param>
    /// <returns>All humidexes within the time interval; otherwise an empty collection.</returns>
    public Task<ICollection<Humidex>> GetHumidexesAsync(DateTime startTime, DateTime endTime);

    /// <summary>
    /// Get the latest humidex value.
    /// </summary>
    /// <returns>Latest humidex; otherwise null.</returns>
    public Task<Humidex> GetLatestHumidexAsync();
}
