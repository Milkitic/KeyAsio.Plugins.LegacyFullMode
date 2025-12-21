using KeyAsio.Audio.Caching;
using KeyAsio.Plugins.Abstractions;
using KeyAsio.Plugins.LegacyFullMode.Tracks;
using KeyAsio.Shared;
using KeyAsio.Shared.OsuMemory;
using KeyAsio.Shared.Sync.Services;
using Microsoft.Extensions.Logging;

namespace KeyAsio.Plugins.LegacyFullMode.States;

public class PlayingState : IGameStateHandler
{
    private static readonly long MusicSyncIntervalTicks = System.Diagnostics.Stopwatch.Frequency / 1000;

    private readonly PauseStatus _pauseStatus;
    private readonly SongPreviewPlayer _songPreviewPlayer;
    private readonly SynchronizedMusicPlayer _synchronizedMusicPlayer;
    private readonly GameplaySessionManager _gameplaySessionManager;
    private readonly AudioCacheManager _audioCacheManager;
    private readonly ILogger _logger;

    private bool _enableMixSync;

    private long _lastMusicSyncTimestamp;
    private int _lastPlayTime;

    public bool FirstStartInitialized { get; set; }

    public PlayingState(
        PauseStatus pauseStatus,
        SongPreviewPlayer songPreviewPlayer,
        SynchronizedMusicPlayer synchronizedMusicPlayer,
        GameplaySessionManager gameplaySessionManager,
        AudioCacheManager audioCacheManager,
        AppSettings appSettings,
        ILogger logger)
    {
        _pauseStatus = pauseStatus;
        _songPreviewPlayer = songPreviewPlayer;
        _synchronizedMusicPlayer = synchronizedMusicPlayer;
        _gameplaySessionManager = gameplaySessionManager;
        _audioCacheManager = audioCacheManager;
        _logger = logger;
        _enableMixSync = appSettings.Sync.EnableMixSync;
        appSettings.Sync.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AppSettings.Sync.EnableMixSync))
            {
                _enableMixSync = appSettings.Sync.EnableMixSync;
            }
        };
    }

    public int Priority => 10;

    public HandleResult HandleEnter(ISyncContext context)
    {
        _lastMusicSyncTimestamp = 0;
        _lastPlayTime = int.MaxValue;
        _songPreviewPlayer.StartLowPass(200, 800);
        return HandleResult.Continue;
    }

    public HandleResult HandleTick(ISyncContext context)
    {
        var enableMixSync = _enableMixSync;
        if (enableMixSync)
        {
            _pauseStatus.UpdatePauseCount(context.PlayTime == _lastPlayTime);
        }

        if (!context.IsStarted) return HandleResult.Continue;

        var currMs = context.PlayTime;
        var prevMs = _lastPlayTime;
        _lastPlayTime = currMs;

        // Retry: song time moved backward during playing
        if (prevMs > currMs && prevMs != 0)
        {
            OnRetry(context, enableMixSync);
            return HandleResult.Continue;
        }

        var timestamp = context.LastUpdateTimestamp;

        // Logic for Music
        if (timestamp - _lastMusicSyncTimestamp >= MusicSyncIntervalTicks)
        {
            if (!enableMixSync) return HandleResult.Continue;
            try
            {
                SyncMusic(context, currMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing music.");
            }

            _lastMusicSyncTimestamp = timestamp;
        }

        return HandleResult.Continue;
    }

    public HandleResult HandleExit(ISyncContext context)
    {
        return HandleResult.Continue;
    }

    private void OnRetry(ISyncContext ctx, bool enableMixSync)
    {
        if (enableMixSync)
        {
            _pauseStatus.ResetPauseState();
            _ = _songPreviewPlayer.StopCurrentMusic();
            _songPreviewPlayer.StartLowPass(200, 16000);
            FirstStartInitialized = true;
            _synchronizedMusicPlayer.ClearAudio();
        }
    }

    private void SyncMusic(ISyncContext ctx, int newMs)
    {
        const int playingPauseThreshold = 5;
        if (!FirstStartInitialized) return;
        if (_gameplaySessionManager.OsuFile == null) return;

        var folder = _gameplaySessionManager.BeatmapFolder;
        var filename = _gameplaySessionManager.AudioFilename;

        if (folder == null || filename == null) return;

        if (_pauseStatus.PauseCount >= playingPauseThreshold)
        {
            _synchronizedMusicPlayer.ClearAudio();
            return;
        }

        var musicPath = Path.Combine(folder, filename);
        if (!_audioCacheManager.TryGet(musicPath, out var cachedAudio)) return;

        const int codeLatency = -1;
        const int osuForceLatency = 15;
        var oldMapForceOffset = _gameplaySessionManager.OsuFile.Version < 5 ? 24 : 0;
        _synchronizedMusicPlayer.Offset = osuForceLatency + codeLatency + oldMapForceOffset;
        _synchronizedMusicPlayer.LeadInMilliseconds = _gameplaySessionManager.OsuFile.General.AudioLeadIn;

        _synchronizedMusicPlayer.PlayMods = (Mods)ctx.PlayMods;

        _synchronizedMusicPlayer.SyncAudio(cachedAudio, newMs);
    }
}