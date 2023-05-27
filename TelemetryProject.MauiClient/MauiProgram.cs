using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using TelemetryProject.CommonClient;
using TelemetryProject.MauiClient.Pages;
using TelemetryProject.MauiClient.Services;
using TelemetryProject.MauiClient.ViewModels;

namespace TelemetryProject.MauiClient;

public static class MauiProgram
{
    private static IServiceProvider _serviceProvider;

    public static TService GetService<TService>()
        => _serviceProvider.GetRequiredService<TService>();

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services.
        builder.Services.AddSingleton<IHttpClientService, HttpClientService>();
        builder.Services.AddSingleton<IHumidexService, HumidexService>();
        builder.Services.AddSingleton<IBoardService, BoardService>();
        builder.Services.AddSingleton<IFileBasedStorageService, FileBasedStorageService>();
        builder.Services.AddSingleton<ISignalRClientService, SignalRClientService>();
        
        // Pages.
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<ChartsPage>();

        // ViewModels for ContentPages.
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<ChartsViewModel>();

        // ViewModels for ContentViews.
        builder.Services.AddTransient<LatestHumidexViewModel>();
        builder.Services.AddTransient<LedControlViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        _serviceProvider = app.Services; // Store service provider reference.

        return app;
    }
}
