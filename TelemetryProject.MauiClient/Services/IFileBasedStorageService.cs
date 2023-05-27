namespace TelemetryProject.MauiClient.Services;

public interface IFileBasedStorageService
{
    /// <summary>
    /// Save an object to the local cache directory.
    /// </summary>
    /// <typeparam name="T">Type of object to store.</typeparam>
    /// <param name="fileName">Name of file to create.</param>
    /// <param name="obj">Object to store.</param>
    public void Store<T>(string fileName, T obj) where T : class;

    /// <summary>
    /// Retreive an object from the local cache directory.
    /// </summary>
    /// <typeparam name="T">Type of stored object.</typeparam>
    /// <param name="fileName">Name of file to read from.</param>
    /// <returns>Deserialized object.</returns>
    public T Retreive<T>(string fileName) where T : class;
}
