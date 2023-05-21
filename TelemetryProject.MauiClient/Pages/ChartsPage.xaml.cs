using MauiClient.ViewModels;

namespace MauiClient.Pages;

public partial class ChartsPage : ContentPage
{
	public ChartsPage(ChartsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
