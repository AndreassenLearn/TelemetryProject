using TelemetryProject.CommonClient.Services;

namespace TelemetryProject.BlazorClient.Services
{
    public class LocalStorageService : IStorageService
    {
        public T Retreive<T>(string fileName) where T : class
        {
            return default(T);
        }

        public void Store<T>(string fileName, T obj) where T : class
        {
            return;
        }
    }
}
