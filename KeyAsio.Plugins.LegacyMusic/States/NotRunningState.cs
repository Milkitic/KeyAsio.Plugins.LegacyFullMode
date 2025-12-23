using KeyAsio.Plugins.Abstractions;
using KeyAsio.Plugins.LegacyMusic.Tracks;
using KeyAsio.Shared;

namespace KeyAsio.Plugins.LegacyMusic.States;

public class NotRunningState : IGameStateHandler
{
    private readonly AppSettings _appSettings;
    private readonly SongPreviewPlayer _songPreviewPlayer;

    public NotRunningState(AppSettings appSettings, SongPreviewPlayer songPreviewPlayer)
    {
        _appSettings = appSettings;
        _songPreviewPlayer = songPreviewPlayer;
    }

    public int Priority => 10;

    public HandleResult HandleEnter(ISyncContext context)
    {
        if (_appSettings.Sync.EnableMixSync)
        {
            _ = _songPreviewPlayer.StopCurrentMusic(2000);
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