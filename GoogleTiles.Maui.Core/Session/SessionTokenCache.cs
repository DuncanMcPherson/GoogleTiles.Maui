using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Session;

internal class SessionTokenCache
{
    private readonly ITimeProvider _timeProvider;
    private SessionToken? _current;

    public SessionTokenCache(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public SessionToken? Current => HasValidToken ? _current : null;

    public bool HasValidToken => _current is not null && !_current.IsExpired(_timeProvider);

    public void Store(SessionToken token)
    {
        _current = token;
    }

    public void Clear()
    {
        _current = null;
    }
}