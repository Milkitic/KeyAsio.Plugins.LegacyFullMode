using Coosu.Beatmap;
using KeyAsio.Core.Audio.Caching;
using KeyAsio.Plugins.Abstractions;
using KeyAsio.Plugins.Abstractions.OsuMemory;
using KeyAsio.Plugins.LegacyFullMode.States;
using KeyAsio.Plugins.LegacyFullMode.Tracks;
using KeyAsio.Shared;
using KeyAsio.Shared.Sync.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KeyAsio.Plugins.LegacyFullMode;

public class LegacyFullModePlugin : ISyncPlugin, IMusicManagerPlugin
{
    public event EventHandler? OptionStateChanged;

    public string Id => "KeyAsio.Plugins.LegacyFullMode";
    public string Name => "Legacy Realtime.FullMode";
    public string Version => "4.0.0";
    public string Author => "KeyAsio Team";
    public string Description => "Provides legacy(v3) music synchronization logic.";
    public string OptionName => "MIX";
    public string OptionTag => "Legacy";
    public int OptionPriority => 10;
    public bool CanEnableOption => true;
  
    private IPluginContext? _context;
    private SynchronizedMusicPlayer? _synchronizedMusicPlayer;
    private SongPreviewPlayer? _songPreviewPlayer;

    private GameplaySessionManager? _gameplaySessionManager;
    private AudioCacheManager? _audioCacheManager;
    private AppSettings? _appSettings;
    private ILogger<LegacyFullModePlugin>? _logger;

    public void Initialize(IPluginContext context)
    {
        _context = context;
        var sp = context.ServiceProvider;
        _appSettings = sp.GetRequiredService<AppSettings>();
        _gameplaySessionManager = sp.GetRequiredService<GameplaySessionManager>();
        _gameplaySessionManager.SessionStopped += OnSessionStopped;
        _audioCacheManager = sp.GetRequiredService<AudioCacheManager>();
        _logger = sp.GetRequiredService<ILogger<LegacyFullModePlugin>>();

        var syncLogger = sp.GetRequiredService<ILogger<SynchronizedMusicPlayer>>();
        var previewLogger = sp.GetRequiredService<ILogger<SongPreviewPlayer>>();

        _synchronizedMusicPlayer = new SynchronizedMusicPlayer(syncLogger, context.AudioEngine, _appSettings);
        _songPreviewPlayer = new SongPreviewPlayer(previewLogger, context.AudioEngine, _appSettings);
        var pauseStatus = new PauseStatus();
        var musicState = new PlayingState(pauseStatus,
            _songPreviewPlayer,
            _synchronizedMusicPlayer,
            _gameplaySessionManager,
            _audioCacheManager,
            _appSettings,
            _logger
        );
        context.RegisterStateHandler(SyncOsuStatus.Playing, musicState);

        var musicBrowsingState = new BrowsingState(_appSettings, _songPreviewPlayer, pauseStatus);
        context.RegisterStateHandler(SyncOsuStatus.ResultsScreen, new ResultsState(_synchronizedMusicPlayer));
        context.RegisterStateHandler(SyncOsuStatus.NotRunning, new NotRunningState(_appSettings, _songPreviewPlayer));
        context.RegisterStateHandler(SyncOsuStatus.SongSelection, musicBrowsingState);
        context.RegisterStateHandler(SyncOsuStatus.EditSongSelection, musicBrowsingState);
        context.RegisterStateHandler(SyncOsuStatus.MainView, musicBrowsingState);
        context.RegisterStateHandler(SyncOsuStatus.MultiSongSelection, musicBrowsingState);
    }

    public void Startup()
    {
    }

    public void Shutdown()
    {
        if (_gameplaySessionManager != null)
        {
            _gameplaySessionManager.SessionStopped -= OnSessionStopped;
        }

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

    private void OnSessionStopped()
    {
        _synchronizedMusicPlayer?.ClearAudio();

        var manager = _gameplaySessionManager;
        if (manager is not { OsuFile: not null, BeatmapFolder: not null }) return;

        var audioPath = manager.OsuFile.General.AudioFilename == null
            ? null
            : Path.Combine(manager.BeatmapFolder, manager.OsuFile.General.AudioFilename);
        _songPreviewPlayer?.Play(manager.OsuFile, audioPath, manager.OsuFile.General.PreviewTime);
    }
}