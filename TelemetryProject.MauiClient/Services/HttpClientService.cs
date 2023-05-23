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
        public async Task<HttpResponseMessage> GetAsync(string uri) => await Policy
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

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> PostAsync(string uri, HttpContent httpContent = null) => await Policy
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
}
