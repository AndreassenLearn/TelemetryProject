using TelemetryProject.MauiClient.ViewModels;

namespace TelemetryProject.MauiClient.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
