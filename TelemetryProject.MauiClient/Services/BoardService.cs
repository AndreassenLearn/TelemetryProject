namespace TelemetryProject.MauiClient.Services;

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
        await _httpClientService.PostAsync($"led/{state}");
    }

    /// <inheritdoc/>
    public async Task SetServoAsync(ushort position)
    {
        await _httpClientService.PostAsync($"servo/{position}");
    }
}
