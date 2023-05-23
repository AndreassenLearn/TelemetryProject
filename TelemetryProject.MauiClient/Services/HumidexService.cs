using Common.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace MauiClient.Services;

public class HumidexService : IHumidexService
{
    private readonly string _fileName = "latesthumidex.txt";
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
                return result;
            }
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
            _fileStorageService.Store(_fileName, humidex);
        }
        else
        {
            // Try get latest humidex from local storage.
            humidex = _fileStorageService.Retreive<Humidex>(_fileName);
        }

        return humidex;
    }
}
