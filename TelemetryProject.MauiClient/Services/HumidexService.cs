using Common.Models;
using System.Net.Http.Json;

namespace MauiClient.Services;

public class HumidexService : IHumidexService
{
    private readonly string _latestHumidexFileName = "latesthumidex.txt";
    private readonly string _humidexesFileName = "humidexes.txt";
    private readonly IHttpClientService _httpClientService;
    private readonly IFileBasedStorageService _fileStorageService;

    public HumidexService(IHttpClientService httpClientService, IFileBasedStorageService fileStorageService)
    {
        _httpClientService = httpClientService;
        _fileStorageService = fileStorageService;
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
                // Store data in file cache.
                _fileStorageService.Store(_humidexesFileName, result);

                return result;
            }
        }
        else
        {
            // Get data from file cache.
            var humidexes = _fileStorageService.Retreive<ICollection<Humidex>>(_humidexesFileName);
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
                _fileStorageService.Store(_latestHumidexFileName, humidex);
            }
        }
        else
        {
            // Try get latest humidex from local storage.
            humidex = _fileStorageService.Retreive<Humidex>(_latestHumidexFileName);
        }

        return humidex;
    }
}
