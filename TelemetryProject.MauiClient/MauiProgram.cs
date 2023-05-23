using CommunityToolkit.Maui;
using MauiClient.Pages;
using MauiClient.Services;
using MauiClient.ViewModels;
using MauiClient.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace MauiClient;

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
        builder.Services.AddSingleton<IHumidexService, HumidexService>();
        
        // Pages.
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<ChartsPage>();

        // ViewModels for ContentPages.
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<ChartsViewModel>();

        // ViewModels for ContentViews.
        builder.Services.AddTransient<LatestHumidexViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        _serviceProvider = app.Services; // Store service provider reference.

        return app;
    }
}
