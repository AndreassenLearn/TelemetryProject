using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiClient.ViewModels;

public partial class LedControlViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _currentState;

    public LedControlViewModel()
    {
        _currentState = false;
    }

    partial void OnCurrentStateChanged(bool value)
    {

    }
}
