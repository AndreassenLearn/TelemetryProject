using System.Globalization;
using System.Net.Http.Json;
using TelemetryProject.Common.Models;

namespace TelemetryProject.CommonClient.Services;

public class HumidexService : IHumidexService
{
    private readonly string _latestHumidexFileName = "latesthumidex.txt";
    private readonly string _humidexesFileName = "humidexes.txt";
    private readonly IHttpClientService _httpClientService;
    private readonly IStorageService _storageService;

    public HumidexService(IHttpClientService httpClientService, IStorageService fileStorageService)
    {
        _httpClientService = httpClientService;
        _storageService = fileStorageService;
    }

    /// <inheritdoc/>
    public async Task<ICollection<Humidex>> GetHumidexesAsync(DateTime startTime, DateTime endTime)
    {
        var response = await _httpClientService.GetAsync($"humidex/{startTime.ToString("yyyy-MM-dTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)}/{endTime.ToString("yyyy-MM-dTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)}");
        if (response?.IsSuccessStatusCode ?? false)
        {
            var result = await response.Content.ReadFromJsonAsync<ICollection<Humidex>>();
            if (result != null)
            {
                // Store data in file cache.
                _storageService.Store(_humidexesFileName, result);

                return result;
            }
        }
        else
        {
            // Get data from file cache.
            var humidexes = _storageService.Retreive<ICollection<Humidex>>(_humidexesFileName);
            var filteredHumidexes = new List<Humidex>();

            if (humidexes != null)
            {
                foreach (var humidex in humidexes)
                {
                    int startResult = DateTime.Compare(humidex.Time, startTime);
                    int endResult = DateTime.Compare(humidex.Time, endTime);
                    if (startResult == 0 || endResult == 0 || (startResult > 0 && endResult < 0))
                        filteredHumidexes.Add(humidex);
                }
            }

            return filteredHumidexes;
        }

        return new List<Humidex>();
    }

    /// <inheritdoc/>
    public async Task<Humidex> GetLatestHumidexAsync()
    {
        Humidex humidex;

        var response = await _httpClientService.GetAsync("humidex/latest");
        if (response?.IsSuccessStatusCode ?? false)
        {
            humidex = await response.Content.ReadFromJsonAsync<Humidex>();

            // Store latest humidex.
            if (humidex != null)
            {
                _storageService.Store(_latestHumidexFileName, humidex);
            }
        }
        else
        {
            // Try get latest humidex from local storage.
            humidex = _storageService.Retreive<Humidex>(_latestHumidexFileName);
        }

        return humidex;
    }
}
