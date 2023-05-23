using Common.Models;
using System.Net.Http.Json;

namespace MauiClient.Services;

public class HumidexService : IHumidexService
{
    private readonly IHttpClientService _httpClientService;

    public HumidexService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    /// <inheritdoc/>
    public async Task<ICollection<Humidex>> GetHumidexesAsync(DateTime startTime, DateTime endTime)
    {
        HttpResponseMessage response = await _httpClientService.GetAsync($"humidex/{startTime:yyyy-MM-dTHH:mm:ss.fffZ}/{endTime:yyyy-MM-dTHH:mm:ss.fffZ}");
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

        HttpResponseMessage response = await _httpClientService.GetAsync("humidex/latest");
        if (response.IsSuccessStatusCode)
        {
            humidex = await response.Content.ReadFromJsonAsync<Humidex>();
        }

        return humidex;
    }
}
