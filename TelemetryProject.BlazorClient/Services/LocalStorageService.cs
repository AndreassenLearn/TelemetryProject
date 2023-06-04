using TelemetryProject.CommonClient.Services;

namespace TelemetryProject.BlazorClient.Services
{
    public class LocalStorageService : IStorageService
    {
        public void Store<T>(string fileName, T obj) where T : class
        {
            return;
        }

        public T? Retreive<T>(string fileName) where T : class
        {
            return null;
        }
    }
}
