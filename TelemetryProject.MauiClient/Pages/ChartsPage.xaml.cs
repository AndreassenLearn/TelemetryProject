using TelemetryProject.MauiClient.ViewModels;

namespace TelemetryProject.MauiClient.Pages;

public partial class ChartsPage : ContentPage
{
	public ChartsPage(ChartsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
