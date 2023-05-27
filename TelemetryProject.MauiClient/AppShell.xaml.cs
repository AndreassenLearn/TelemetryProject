using TelemetryProject.MauiClient.Pages;

namespace TelemetryProject.MauiClient;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(ChartsPage), typeof(ChartsPage));
    }
}
