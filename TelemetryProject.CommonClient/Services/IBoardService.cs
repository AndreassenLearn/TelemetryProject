namespace TelemetryProject.CommonClient.Services;

public interface IBoardService
{
    /// <summary>
    /// Set servo position of the embedded circuit.
    /// </summary>
    /// <param name="position">0 to 180 degrees.</param>
    public Task SetServoAsync(ushort position);

    /// <summary>
    /// Set LED state of the embedded circuit.
    /// </summary>
    /// <param name="state">True for 'on' and false for 'off'.</param>
    public Task SetLedAsync(bool state);
}
