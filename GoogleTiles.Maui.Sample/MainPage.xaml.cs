using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Sample.ViewModels;

namespace GoogleTiles.Maui.Sample;

public partial class MainPage : ContentPage
{
    private IDispatcherTimer? _locationTimer;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            StartTimer();
        });
    }

    private void StartTimer()
    {
        if (_locationTimer is null)
        {
            _locationTimer = Dispatcher.CreateTimer();
            _locationTimer.Tick += _locationTimer_Tick;
            _locationTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _locationTimer.Start();
        }
    }

    private void _locationTimer_Tick(object? sender, EventArgs e)
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var location = await Geolocation.GetLocationAsync();
            if (location is null)
                return;
            GTView.Location?.UpdateLocation(new GeoCoordinate(location.Latitude, location.Longitude), location.Course);
        });
    }
}