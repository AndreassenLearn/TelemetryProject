using Common.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiClient.ViewModels
{
    public partial class LatestHumidexViewModel : ObservableObject
    {
        [ObservableProperty]
        private Humidex _humidex;
    }
}
