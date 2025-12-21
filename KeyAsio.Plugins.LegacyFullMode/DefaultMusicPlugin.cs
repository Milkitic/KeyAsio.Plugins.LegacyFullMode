using Coosu.Beatmap;
using KeyAsio.Audio.Caching;
using KeyAsio.Plugins.Abstractions;
using KeyAsio.Plugins.LegacyFullMode.States;
using KeyAsio.Plugins.LegacyFullMode.Tracks;
using KeyAsio.Shared;
using KeyAsio.Shared.OsuMemory;
using KeyAsio.Shared.Plugins;
using KeyAsio.Shared.Sync.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KeyAsio.Plugins.LegacyFullMode;

public class DefaultMusicPlugin : ISyncPlugin, IMusicManagerPlugin
{
    public string Id => "KeyAsio.Plugins.LegacyFullMode";
    public string Name => "Legacy Realtime.FullMode";
    public string Version => "4.0.0";
    public string Author => "KeyAsio Team";
    public string Description => "Provides legacy(v3) music synchronization logic.";

    private IPluginContext? _context;
    private SynchronizedMusicPlayer? _synchronizedMusicPlayer;
    private SongPreviewPlayer? _songPreviewPlayer;

    private GameplaySessionManager? _gameplaySessionManager;
    private AudioCacheManager? _audioCacheManager;
    private BackgroundMusicManager? _backgroundMusicManager;
    private AppSettings? _appSettings;
    private ILogger<DefaultMusicPlugin>? _logger;

    public void Initialize(IPluginContext context)
    {
        _context = context;
        var sp = context.ServiceProvider;
        _appSettings = sp.GetRequiredService<AppSettings>();
        _gameplaySessionManager = sp.GetRequiredService<GameplaySessionManager>();
        _audioCacheManager = sp.GetRequiredService<AudioCacheManager>();
        _backgroundMusicManager = sp.GetRequiredService<BackgroundMusicManager>();
        _logger = sp.GetRequiredService<ILogger<DefaultMusicPlugin>>();

        var syncLogger = sp.GetRequiredService<ILogger<SynchronizedMusicPlayer>>();
        var previewLogger = sp.GetRequiredService<ILogger<SongPreviewPlayer>>();

        _synchronizedMusicPlayer = new SynchronizedMusicPlayer(syncLogger, context.AudioEngine, _appSettings);
        _songPreviewPlayer = new SongPreviewPlayer(previewLogger, context.AudioEngine, _appSettings);

        var musicState = new MusicPlayingState(
            _backgroundMusicManager,
            _gameplaySessionManager,
            _audioCacheManager,
            _appSettings,
            _logger
        );
        context.RegisterStateHandler(SyncOsuStatus.Playing, musicState);
    }

    public void Startup()
    {
    }

    public void Shutdown()
    {
        _ = _songPreviewPlayer?.StopCurrentMusic();
        _synchronizedMusicPlayer?.ClearAudio();
    }

    public void Unload()
    {
    }

    public void OnSyncStart()
    {
    }

    public void OnSyncStop()
    {
    }

    public void OnTick(ISyncContext context, int deltaMs)
    {
    }

    public void OnStatusChanged(SyncOsuStatus oldStatus, SyncOsuStatus newStatus)
    {
    }

    public void OnBeatmapChanged(SyncBeatmapInfo beatmap)
    {
    }

    // IMusicManagerPlugin implementation
    public void StartLowPass(int fadeMilliseconds, int targetFrequency)
        => _songPreviewPlayer?.StartLowPass(fadeMilliseconds, targetFrequency);

    public void StopCurrentMusic(int fadeMs = 0)
        => _ = _songPreviewPlayer?.StopCurrentMusic(fadeMs);

    public void PauseCurrentMusic()
        => _ = _songPreviewPlayer?.PauseCurrentMusic();

    public void RecoverCurrentMusic()
        => _ = _songPreviewPlayer?.RecoverCurrentMusic();

    public void PlaySingleAudioPreview(OsuFile osuFile, string? path, int playTime)
        => _ = _songPreviewPlayer?.Play(osuFile, path, playTime);

    public void SetSingleTrackPlayMods(Mods mods)
    {
        if (_synchronizedMusicPlayer != null) _synchronizedMusicPlayer.PlayMods = mods;
    }

    public void SetMainTrackOffsetAndLeadIn(int offset, int leadInMs)
    {
        if (_synchronizedMusicPlayer != null)
        {
            _synchronizedMusicPlayer.Offset = offset;
            _synchronizedMusicPlayer.LeadInMilliseconds = leadInMs;
        }
    }

    public void SyncMainTrackAudio(CachedAudio sound, int positionMs)
        => _synchronizedMusicPlayer?.SyncAudio(sound, positionMs);

    public void ClearMainTrackAudio()
        => _synchronizedMusicPlayer?.ClearAudio();
}