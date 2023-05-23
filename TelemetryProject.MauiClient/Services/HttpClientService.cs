using Polly;
using System.Diagnostics;

namespace MauiClient.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _client;

        public HttpClientService()
        {
            _client = new HttpClient() { BaseAddress = new Uri(Constants.ApiBaseUrl) };
        }

        public HttpClient Client => _client;

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            try
            {
                return await Policy
                    .HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                    .WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(5), onRetry: (result, time) =>
                    {
                        Debug.WriteLine($"{nameof(GetAsync)}: Retrying in {time} ...");
                    })
                    .ExecuteAsync(async () =>
                    {
                        Debug.WriteLine($"{nameof(GetAsync)}: {Client.BaseAddress}{uri}");
                        return await Client.GetAsync(uri);
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(GetAsync)} failed: {ex.Message}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> PostAsync(string uri, HttpContent httpContent = null)
        {
            try
            {
                return await Policy
                    .HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                    .WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(5), onRetry: (result, time) =>
                    {
                        Debug.WriteLine($"{nameof(GetAsync)}: Retrying in {time} ...");
                    })
                    .ExecuteAsync(async () =>
                    {
                        Debug.WriteLine($"{nameof(PostAsync)}: {Client.BaseAddress}{uri}");
                        return await Client.PostAsync(uri, httpContent);
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(PostAsync)} failed: {ex.Message}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
