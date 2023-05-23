namespace MauiClient.Services;

public interface IHttpClientService
{
    public HttpClient Client { get; }

    /// <summary>
    /// Send a GET message using the base address of <see cref="Client"/>.
    /// </summary>
    /// <param name="uri">URI (must NOT start with '/').</param>
    /// <returns>HTTP response.</returns>
    public Task<HttpResponseMessage> GetAsync(string uri);

    /// <summary>
    /// Send a POST message using the base address of <see cref="Client"/>.
    /// </summary>
    /// <param name="uri">URI (must NOT start with '/').</param>
    /// <param name="httpContent">Content of the POST message.</param>
    /// <returns>HTTP response.</returns>
    public Task<HttpResponseMessage> PostAsync(string uri, HttpContent httpContent = null);
}
