using KeyAsio.Plugins.Abstractions;
using KeyAsio.Shared;
using KeyAsio.Shared.Sync.Services;

namespace KeyAsio.Plugins.LegacyFullMode.States;

public class NotRunningState : IGameStateHandler
{
    private readonly AppSettings _appSettings;
    private readonly BackgroundMusicManager _backgroundMusicManager;

    public NotRunningState(AppSettings appSettings, BackgroundMusicManager backgroundMusicManager)
    {
        _appSettings = appSettings;
        _backgroundMusicManager = backgroundMusicManager;
    }

    public int Priority => 10;

    public HandleResult HandleEnter(ISyncContext context)
    {
        if (_appSettings.Sync.EnableMixSync)
        {
            _backgroundMusicManager.StopCurrentMusic(2000);
        }

        return HandleResult.Continue;
    }

    public HandleResult HandleExit(ISyncContext context)
    {
        return HandleResult.Continue;
    }

    public HandleResult HandleTick(ISyncContext context)
    {
        return HandleResult.Continue;
    }

    public HandleResult HandleBeatmapChange(SyncBeatmapInfo beatmap)
    {
        return HandleResult.Continue;
    }
}