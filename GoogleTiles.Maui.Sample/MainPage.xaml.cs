using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Layers;
using GoogleTiles.Maui.Models;
using GoogleTiles.Maui.Sample.ViewModels;

namespace GoogleTiles.Maui.Sample;

public partial class MainPage : ContentPage
{
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
        });
    }
}