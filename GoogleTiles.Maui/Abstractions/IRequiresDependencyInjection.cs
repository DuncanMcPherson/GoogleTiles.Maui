namespace GoogleTiles.Maui.Abstractions;

internal interface IRequiresDependencyInjection
{
    void InjectDependencies(IServiceProvider services);
}