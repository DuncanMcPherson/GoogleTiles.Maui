using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GoogleTiles.Maui.Core.Models;

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

    public bool IsNightEnabled => MapType == MapType.Roadmap;

    public ICommand CycleMapTypeCommand { get; private set; }
    public ICommand ToggleNightMode { get; private set; }

    public MainViewModel()
    {
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