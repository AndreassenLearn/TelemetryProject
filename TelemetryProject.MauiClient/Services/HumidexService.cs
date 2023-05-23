using Common.Models;
using System.Net.Http.Json;

namespace MauiClient.Services;

public class HumidexService : IHumidexService
{
    private readonly HttpClient _client;

    public HumidexService()
    {
        _client = new HttpClient();
    }

    /// <inheritdoc/>
    public async Task<ICollection<Humidex>> GetHumidexesAsync(DateTime startTime, DateTime endTime)
    {
        Uri uri = new(string.Format(Constants.ApiUrl, $"humidex/{startTime:yyyy-MM-dTHH:mm:ss.fffZ}/{endTime:yyyy-MM-dTHH:mm:ss.fffZ}"));
        var response = await _client.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ICollection<Humidex>>();
        }

        return new List<Humidex>();
    }

    /// <inheritdoc/>
    public async Task<Humidex> GetLatestHumidexAsync()
    {
        Humidex humidex = null;

        Uri uri = new(string.Format(Constants.ApiUrl, "humidex/latest"));
        var response = await _client.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            humidex = await response.Content.ReadFromJsonAsync<Humidex>();
        }

        return humidex;
    }
}
