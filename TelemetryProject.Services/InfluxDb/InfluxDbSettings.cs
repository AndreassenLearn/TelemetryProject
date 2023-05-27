namespace TelemetryProject.Services.InfluxDb
{
    public class InfluxDbSettings
    {
        public string OrganizationId { get; set; } = string.Empty;
        public string Bucket { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
