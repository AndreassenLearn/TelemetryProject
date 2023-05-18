using Common.Models;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClient.Views;
using System.Collections.ObjectModel;

namespace MauiClient.ViewModels;

public partial class ChartsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Humidex> _humidexes = new();

    [ObservableProperty]
    private DateTime _minTime = DateTime.MinValue;

    [ObservableProperty]
    private DateTime _maxTime = DateTime.MaxValue;

    [ObservableProperty]
    private DateTime _startTime;

    [ObservableProperty]
    private DateTime _endTime;

    public ChartsViewModel()
    {
        int count = 10;
        for (int i = 0; i < count; i++)
        {
            _humidexes.Add(new()
            {
                Temperature = RandomFloat(22, 28),
                Humidity = RandomFloat(30, 60),
                Time = new DateTime(2023, 12, 31, RandomInteger(0, 12), RandomInteger(0, 59), 0)
            });
        }
    }

    [RelayCommand]
    public async void UpdateHumidexGraph()
    {
        LoadingPopup loadingPopup = new();
        try
        {
            Page page = Application.Current?.MainPage ?? throw new NullReferenceException();
            page.ShowPopup(loadingPopup);

            await new TaskFactory().StartNew(() => { Thread.Sleep(5000); });
        }
        catch (Exception)
        {

        }
        finally
        {
            loadingPopup.Close();
        }
    }

    private static float RandomFloat(float min, float max) => (float)new Random().NextDouble() * (max - min) + min;

    private static int RandomInteger(int min, int max) => new Random().Next(min, max);
}
