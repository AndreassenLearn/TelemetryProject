namespace TelemetryProject.Common;

public record Constants
{
    public record SignalR
    {
        public readonly static string HumidexHubUri = "notify/humidex";

        public readonly static string LiveHumidexMethodName = "LiveHumidex";
    }
}
