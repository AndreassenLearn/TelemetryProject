using CommunityToolkit.Mvvm.ComponentModel;
using TelemetryProject.CommonClient.Services;

namespace TelemetryProject.MauiClient.ViewModels;

public partial class LedControlViewModel : ObservableObject
{
    private readonly IBoardService _boardService;

    [ObservableProperty]
    private bool _currentState;

    public LedControlViewModel() : this(MauiProgram.GetService<IBoardService>())
    { }

    public LedControlViewModel(IBoardService boardService)
    {
        _currentState = false;
        _boardService = boardService;
    }

    partial void OnCurrentStateChanged(bool value)
    {
        _boardService.SetLedAsync(value);
    }
}
