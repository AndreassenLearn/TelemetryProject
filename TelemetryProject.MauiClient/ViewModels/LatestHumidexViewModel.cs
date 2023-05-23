using Common.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClient.Services;

namespace MauiClient.ViewModels
{
    public partial class LatestHumidexViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _doShowData = false;

        [ObservableProperty]
        private bool _isRefreshing = false;

        [ObservableProperty]
        private Humidex _humidex;

        private readonly IHumidexService _humidexService;

        public LatestHumidexViewModel() : this(MauiProgram.GetService<IHumidexService>())
        { }

        public LatestHumidexViewModel(IHumidexService humidexService)
        {
            _humidexService = humidexService;
            MainThread.BeginInvokeOnMainThread(Refresh);
        }

        [RelayCommand]
        public async void Refresh()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;

                var humidex = await _humidexService.GetLatestHumidexAsync();
                if (humidex != null)
                {
                    Humidex = humidex;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                IsRefreshing = false;
                DoShowData = true;
            }
        }
    }
}
