using Common.Models;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClient.Services;
using MauiClient.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MauiClient.ViewModels;

public partial class ChartsViewModel : ObservableObject
{
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

    private readonly IHumidexService _humidexService;

    public ChartsViewModel(IHumidexService humidexService)
    {
        _humidexService = humidexService;
        UpdateHumidexGraph();
    }

    [RelayCommand]
    public async void UpdateHumidexGraph()
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
                Humidexes.Add(humidex);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{nameof(UpdateHumidexGraph)} failed: {ex.Message}");
        }
        finally
        {
            loadingPopup.Close();
        }
    }
}
