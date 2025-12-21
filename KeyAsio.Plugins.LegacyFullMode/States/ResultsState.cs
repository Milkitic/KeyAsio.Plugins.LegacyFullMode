using KeyAsio.Plugins.Abstractions;
using KeyAsio.Shared.OsuMemory;
using KeyAsio.Shared.Sync.Services;

namespace KeyAsio.Plugins.LegacyFullMode.States;

public class ResultsState : IGameStateHandler
{
    private readonly BackgroundMusicManager _backgroundMusicManager;

    public ResultsState(BackgroundMusicManager backgroundMusicManager)
    {
        _backgroundMusicManager = backgroundMusicManager;
    }

    public int Priority => 10;

    public HandleResult HandleEnter(ISyncContext context)
    {
        _backgroundMusicManager.SetSingleTrackPlayMods(Mods.None);
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