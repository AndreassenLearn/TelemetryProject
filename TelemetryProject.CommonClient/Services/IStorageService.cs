namespace TelemetryProject.CommonClient.Services;

public interface IStorageService
{
    /// <summary>
    /// Save an object to the local cache directory.
    /// </summary>
    /// <typeparam name="T">Type of object to store.</typeparam>
    /// <param name="name">Name of object to store.</param>
    /// <param name="obj">Object to store.</param>
    public void Store<T>(string name, T obj) where T : class;

    /// <summary>
    /// Retreive an object from the local cache directory.
    /// </summary>
    /// <typeparam name="T">Type of stored object.</typeparam>
    /// <param name="name">Name of object to read.</param>
    /// <returns>Deserialized object; otherwise null.</returns>
    public T Retreive<T>(string name) where T : class;
}
