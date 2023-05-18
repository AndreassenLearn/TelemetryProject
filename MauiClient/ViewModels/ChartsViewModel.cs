using Common.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MauiClient.ViewModels;

public partial class ChartsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Humidex> _humidexes = new();

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
    
    private static float RandomFloat(float min, float max) => (float)new Random().NextDouble() * (max - min) + min;

    private static int RandomInteger(int min, int max) => new Random().Next(min, max);
}
