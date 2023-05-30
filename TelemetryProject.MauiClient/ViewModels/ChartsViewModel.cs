using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TelemetryProject.Common.Models;
using TelemetryProject.CommonClient.Services;
using TelemetryProject.MauiClient.Services;
using TelemetryProject.MauiClient.Views;

namespace TelemetryProject.MauiClient.ViewModels;

public partial class ChartsViewModel : ObservableObject
{
    private static readonly string _stopSyncText = "Stop";
    private static readonly string _startSyncText = "Synchronize Live";

    [ObservableProperty]
    private ObservableCollection<Humidex> _humidexes = new();

    [ObservableProperty]
    private DateTime _minDate = DateTime.MinValue;

    [ObservableProperty]
    private DateTime _maxDate = DateTime.MaxValue;

    [ObservableProperty]
    private DateTime _startDate = DateTime.UtcNow;

    [ObservableProperty]
    private DateTime _endDate = DateTime.UtcNow;

    [ObservableProperty]
    private TimeSpan _startTime = DateTime.UtcNow.TimeOfDay - TimeSpan.FromHours(1);

    [ObservableProperty]
    private TimeSpan _endTime = DateTime.UtcNow.TimeOfDay;

    [ObservableProperty]
    private bool _useEndTime = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateHumidexGraphAsyncCommand))]
    private bool _isSynchronizing = false;

    [ObservableProperty]
    private string _syncButtonText = _startSyncText;

    private readonly IHumidexService _humidexService;
    private readonly ISignalRClientService _signalRClientService;

    public ChartsViewModel(IHumidexService humidexService, ISignalRClientService signalRClientService)
    {
        _humidexService = humidexService;
        _signalRClientService = signalRClientService;
        UpdateHumidexGraphAsync();
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    public async void UpdateHumidexGraphAsync()
    {
        LoadingPopup loadingPopup = new();
        try
        {
            Page page = Application.Current?.MainPage ?? throw new NullReferenceException();
            page.ShowPopup(loadingPopup);

            DateTime start = new(StartDate.Year, StartDate.Month, StartDate.Day, StartTime.Hours, StartTime.Minutes, StartTime.Seconds, DateTimeKind.Utc);

            DateTime end;
            if (UseEndTime)
            {
                end = new(EndDate.Year, EndDate.Month, EndDate.Day, EndTime.Hours, EndTime.Minutes, EndTime.Seconds, DateTimeKind.Utc);
            }
            else
            {
                end = DateTime.MaxValue;
            }

            var humidexes = await _humidexService.GetHumidexesAsync(start, end);

            if (Humidexes.Any())
            {
                Humidexes.Clear();
            }

            foreach (var humidex in humidexes)
            {
                AddSingleHumidex(humidex);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{nameof(UpdateHumidexGraphAsync)} failed: {ex.Message}");
        }
        finally
        {
            loadingPopup.Close();
        }
    }

    [RelayCommand]
    public void ToggleLiveSynchronization()
    {
        if (IsSynchronizing)
        {
            _signalRClientService.StopLiveHumidex();
            IsSynchronizing = false;
            SyncButtonText = _startSyncText;

            return;
        }

        UseEndTime = false;
        StartDate = DateTime.UtcNow;
        StartTime = DateTime.UtcNow.TimeOfDay;
        IsSynchronizing = true;
        SyncButtonText = _stopSyncText;
        _signalRClientService.StartLiveHumidex(AddSingleHumidex);
    }

    private bool CanUpdate() => !IsSynchronizing;

    private void AddSingleHumidex(Humidex humidex)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Humidexes.Add(humidex);
        });
    }
}
