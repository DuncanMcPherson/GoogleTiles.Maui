using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Models;

namespace GoogleTiles.Maui.Sample.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private MapType _mapType = MapType.Roadmap;

    public MapType MapType
    {
        get => _mapType;
        set => SetField(ref _mapType, value);
    }

    private MapTheme _theme = MapTheme.Day;

    public MapTheme Theme
    {
        get => _theme;
        set => SetField(ref _theme, value);
    }

    private int _rotation;

    public int Rotation
    {
        get => _rotation;
        set => SetField(ref _rotation, value);
    }

    public bool IsNightEnabled => MapType == MapType.Roadmap;

    public ICommand CycleMapTypeCommand { get; private set; }
    public ICommand ToggleNightMode { get; private set; }
    public ICommand AddTestPin { get; private set; }
    public ICommand IncreaseRotation { get; private set; }
    public ICommand SetRotation { get; private set; }
    public ICommand AddTestPolyline { get; private set; }
    public ICommand AddTestPathIncremental { get; private set; }

    public MainViewModel()
    {
        AddTestPathIncremental = new Command((view) =>
        {
            if (view is not GoogleTilesView gtv) return;

            var polyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 4
            };
            gtv.Polylines!.Add(polyline);

            var points = new[]
            {
                new GeoCoordinate(40.7608, -111.8910),
                new GeoCoordinate(40.7650, -111.8850),
                new GeoCoordinate(40.7700, -111.8950),
                new GeoCoordinate(40.7750, -111.8830)
            };

            var index = 0;
            Application.Current!.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(750), () =>
            {
                if (index >= points.Length) return false;
                polyline.Positions.Add(points[index]);
                index++;
                return index < points.Length;
            });

        });
        AddTestPolyline = new Command((view) =>
        {
            if (view is not GoogleTilesView gtv) return;
            var polyline = new Polyline
            {
                StrokeColor = Colors.Red,
                StrokeWidth = 6,
                IsClosed = true,
                DashPattern = [8f, 2f, 3f, 2f]
            };

            polyline.Positions.Add(new GeoCoordinate(40.7608, -111.8910));
            polyline.Positions.Add(new GeoCoordinate(40.7650, -111.8850));
            polyline.Positions.Add(new GeoCoordinate(40.7700, -111.8950));
            polyline.Positions.Add(new GeoCoordinate(40.7750, -111.8830));

            gtv.Polylines.Add(polyline);
        });
        SetRotation = new Command((rotation) =>
        {
            if (rotation is not Entry control)
                return;
            if (!int.TryParse(control.Text, out var rotate))
            {
                return;
            }

            if (rotate > 180)
                rotate -= 360;
            Rotation = rotate;
        });
        IncreaseRotation = new Command(() =>
        {
            Rotation++;
            if (Rotation > 180)
            {
                Rotation -= 360;
            }
        });
        AddTestPin = new Command((view) =>
        {
            if (view is not GoogleTilesView gtv) return;

            gtv.Pins.Add(new Pin(new GeoCoordinate(40.7608, -111.8910), null, "Test SLC", true));
        });
        CycleMapTypeCommand = new Command(() =>
        {
            switch (MapType)
            {
                case MapType.Roadmap:
                    MapType = MapType.Satellite;
                    break;
                case MapType.Satellite:
                    MapType = MapType.Roadmap;
                    break;
                case MapType.Terrain:
                case MapType.Streetview:
                default:
                    break;
            }
        });

        ToggleNightMode = new Command(() =>
        {
            switch (Theme)
            {
                case MapTheme.Night:
                    Theme = MapTheme.Day;
                    break;
                case MapTheme.Day:
                case MapTheme.Custom:
                default:
                    Theme = MapTheme.Night;
                    break;
            }
        });
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}