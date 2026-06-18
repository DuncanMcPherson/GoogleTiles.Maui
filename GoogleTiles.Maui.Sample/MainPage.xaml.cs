namespace GoogleTiles.Maui.Sample;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        });
    }
}