using Polly;
using System.Diagnostics;

namespace TelemetryProject.MauiClient.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _client;

        public HttpClientService()
        {
            _client = new HttpClient() { BaseAddress = new Uri(CommonClient.Constants.ApiBaseUrl) };
        }

        public HttpClient Client => _client;

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            try
            {
                CheckNetwork();

                return await Policy
                    .HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                    .WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(0.2 * Math.Pow(2, retryAttempt)), onRetry: (result, time) =>
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
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> PostAsync(string uri, HttpContent httpContent = null)
        {
            try
            {
                CheckNetwork();

                return await Policy
                    .HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                    .WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(0.2 * Math.Pow(2, retryAttempt)), onRetry: (result, time) =>
                    {
                        Debug.WriteLine($"{nameof(PostAsync)}: Retrying in {time} ...");
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
            }

            return null;
        }

        /// <summary>
        /// Check if the device has an active internet connection and throw an exception if not.
        /// </summary>
        /// <exception cref="Exception">No internet access.</exception>
        private static void CheckNetwork()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new Exception("No network access;");
            }
        }
    }
}
