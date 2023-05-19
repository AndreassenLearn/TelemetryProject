using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiClient.ViewModels;

public partial class ServoControlViewModel : ObservableObject
{
    [ObservableProperty]
    private int _minValue = 0;

    [ObservableProperty]
    private int _maxValue = 180;

    [ObservableProperty]
    private int _currentValue;

    public ServoControlViewModel()
    {
        _currentValue = 0;
    }

    [RelayCommand]
    public async void ValueChanged()
    {

    }
}
