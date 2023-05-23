using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClient.Services;

namespace MauiClient.ViewModels;

public partial class ServoControlViewModel : ObservableObject
{
    private readonly IBoardService _boardService;

    [ObservableProperty]
    private ushort _minValue = 0;

    [ObservableProperty]
    private ushort _maxValue = 180;

    [ObservableProperty]
    private ushort _currentValue;

    public ServoControlViewModel() : this(MauiProgram.GetService<IBoardService>())
    { }

    public ServoControlViewModel(IBoardService boardService)
    {
        _boardService = boardService;
        _currentValue = 0;
    }

    [RelayCommand]
    public async void ValueChanged()
    {
        await _boardService.SetServoAsync(CurrentValue);
    }
}
