using Common.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiClient.ViewModels
{
    public partial class LatestHumidexViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isRefreshing = false;

        [ObservableProperty]
        private Humidex _humidex = new() { Temperature = 14.6f, Humidity = 48.83f, Time = DateTime.UtcNow };

        public LatestHumidexViewModel()
        {

        }

        [RelayCommand]
        public async void Refresh()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;

                await new TaskFactory().StartNew(() => { Thread.Sleep(5000); });
            }
            catch (Exception ex)
            {

            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}
