using System.Net.Http.Json;

namespace MauiClient.Services;

public class BoardService : IBoardService
{
    private readonly IHttpClientService _httpClientService;

    public BoardService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    /// <inheritdoc/>
    public async Task SetLedAsync(bool state)
    {
        Uri uri = new(string.Format(Constants.ApiUrl, $"led/{state}"));
        await _httpClientService.Client.PostAsync(uri, null);
    }

    /// <inheritdoc/>
    public async Task SetServoAsync(ushort position)
    {
        Uri uri = new(string.Format(Constants.ApiUrl, $"servo/{position}"));
        await _httpClientService.Client.PostAsync(uri, null);
    }
}
