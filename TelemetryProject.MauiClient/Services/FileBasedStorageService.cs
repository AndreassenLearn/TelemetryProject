using System.Diagnostics;
using System.Text.Json;
using TelemetryProject.CommonClient.Services;

namespace TelemetryProject.MauiClient.Services;

public class FileBasedStorageService : IStorageService
{
    private readonly string _storagePath = FileSystem.Current.CacheDirectory;

    /// <inheritdoc/>
    public void Store<T>(string name, T obj) where T : class
    {
        var path = Path.Combine(_storagePath, name + ".txt");

        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(obj));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{nameof(Store)} failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public T Retreive<T>(string name) where T : class
    {
        var path = Path.Combine(_storagePath, name + ".txt");

        try
        {
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize(File.ReadAllText(path), typeof(T)) as T;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{nameof(Retreive)} failed: {ex.Message}");
        }

        return null;
    }
}
