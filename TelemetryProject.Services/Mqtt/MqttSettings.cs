namespace TelemetryProject.Services.MqttService
{
    public class MqttSettings
    {
        public string Address { get; set; } = string.Empty;
        public bool UseTls { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
