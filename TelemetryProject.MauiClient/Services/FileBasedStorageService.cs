using System.Diagnostics;
using System.Text.Json;

namespace MauiClient.Services;

public class FileBasedStorageService : IFileBasedStorageService
{
    private readonly string _storagePath = FileSystem.Current.CacheDirectory;

    /// <inheritdoc/>
    public T Retreive<T>(string fileName) where T : class
    {
        var path = Path.Combine(_storagePath, fileName);

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

    /// <inheritdoc/>
    public void Store<T>(string fileName, T obj) where T : class
    {
        var path = Path.Combine(_storagePath, fileName);

        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(obj));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{nameof(Store)} failed: {ex.Message}");
        }
    }
}
