using KeyAsio.Plugins.Abstractions;
using KeyAsio.Plugins.Abstractions.OsuMemory;
using KeyAsio.Plugins.LegacyFullMode.Tracks;

namespace KeyAsio.Plugins.LegacyFullMode.States;

public class ResultsState : IGameStateHandler
{
    private readonly SynchronizedMusicPlayer _synchronizedMusicPlayer;

    public ResultsState(SynchronizedMusicPlayer synchronizedMusicPlayer)
    {
        _synchronizedMusicPlayer = synchronizedMusicPlayer;
    }

    public int Priority => 10;

    public HandleResult HandleEnter(ISyncContext context)
    {
        _synchronizedMusicPlayer.PlayMods = Mods.None;
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