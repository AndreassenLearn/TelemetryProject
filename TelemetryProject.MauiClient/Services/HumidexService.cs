using Common.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace MauiClient.Services;

public class HumidexService : IHumidexService
{
    private readonly IHttpClientService _httpClientService;
    private readonly string _latestHumidexPath;

    public HumidexService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
        _latestHumidexPath = Path.Combine(FileSystem.Current.CacheDirectory, "latesthumidex.txt");
    }

    /// <inheritdoc/>
    public async Task<ICollection<Humidex>> GetHumidexesAsync(DateTime startTime, DateTime endTime)
    {
        var response = await _httpClientService.GetAsync($"humidex/{startTime:yyyy-MM-dTHH:mm:ss.fffZ}/{endTime:yyyy-MM-dTHH:mm:ss.fffZ}");
        if (response?.IsSuccessStatusCode ?? false)
        {
            var result = await response.Content.ReadFromJsonAsync<ICollection<Humidex>>();
            if (result != null)
            {
                return result;
            }
        }

        return new List<Humidex>();
    }

    /// <inheritdoc/>
    public async Task<Humidex?> GetLatestHumidexAsync()
    {
        Humidex? humidex = null;

        var response = await _httpClientService.GetAsync("humidex/latest");
        if (response?.IsSuccessStatusCode ?? false)
        {
            humidex = await response.Content.ReadFromJsonAsync<Humidex>();

            // Store latest humidex.
            try
            {
                File.WriteAllText(_latestHumidexPath, JsonSerializer.Serialize(humidex));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(GetLatestHumidexAsync)} failed: {ex.Message}");
            }
        }
        else
        {
            // Try get latest humidex from local storage.
            try
            {
                if (File.Exists(_latestHumidexPath))
                {
                    humidex = JsonSerializer.Deserialize(File.ReadAllText(_latestHumidexPath), typeof(Humidex)) as Humidex;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(GetLatestHumidexAsync)} failed: {ex.Message}");
            }
        }

        return humidex;
    }
}
