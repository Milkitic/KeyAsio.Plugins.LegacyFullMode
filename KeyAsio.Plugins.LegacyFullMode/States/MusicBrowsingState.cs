using Coosu.Beatmap;
using KeyAsio.Plugins.Abstractions;
using KeyAsio.Shared;
using KeyAsio.Shared.Sync.Services;
using Microsoft.Extensions.Logging;

namespace KeyAsio.Plugins.LegacyFullMode.States;

public class MusicBrowsingState : IGameStateHandler
{
    private readonly AppSettings _appSettings;
    private readonly BackgroundMusicManager _backgroundMusicManager;
    private readonly ILogger _logger;

    private string? _lastPreviewAudioPath;
    private bool _isActive;

    public MusicBrowsingState(AppSettings appSettings,
        BackgroundMusicManager backgroundMusicManager,
        ILogger logger)
    {
        _appSettings = appSettings;
        _backgroundMusicManager = backgroundMusicManager;
        _logger = logger;
    }

    public int Priority => 10;

    public HandleResult HandleEnter(ISyncContext context)
    {
        _isActive = true;
        _backgroundMusicManager.StartLowPass(200, 16000);
        return HandleResult.Continue;
    }

    public HandleResult HandleExit(ISyncContext context)
    {
        _isActive = false;
        return HandleResult.Continue;
    }

    public HandleResult HandleTick(ISyncContext context)
    {
        const int selectSongPauseThreshold = 20;
        if (!_appSettings.Sync.EnableMixSync) return HandleResult.Continue;

        // Maintain pause state lifecycle for song-select preview
        _backgroundMusicManager.UpdatePauseCount(context.IsPaused);

        if (_backgroundMusicManager.PauseCount >= selectSongPauseThreshold &&
            _backgroundMusicManager.PreviousSelectSongStatus)
        {
            _backgroundMusicManager.PauseCurrentMusic();
            _backgroundMusicManager.PreviousSelectSongStatus = false;
        }
        else if (_backgroundMusicManager.PauseCount < selectSongPauseThreshold &&
                 !_backgroundMusicManager.PreviousSelectSongStatus)
        {
            _backgroundMusicManager.RecoverCurrentMusic();
            _backgroundMusicManager.PreviousSelectSongStatus = true;
        }

        return HandleResult.Continue;
    }

    public HandleResult HandleBeatmapChange(SyncBeatmapInfo beatmap)
    {
        if (!_isActive) return HandleResult.Continue;

        if (beatmap == default || string.IsNullOrEmpty(beatmap.Folder))
        {
            return HandleResult.Continue;
        }

        var filenameFull = Path.Combine(beatmap.Folder, beatmap.Filename);

        if (!File.Exists(filenameFull)) return HandleResult.Continue;

        // Use Coosu to read the beatmap file
        var coosu = OsuFile.ReadFromFile(filenameFull, k =>
        {
            k.IncludeSection("General");
            k.IncludeSection("Metadata");
        });

        var audioFilePath = coosu.General?.AudioFilename == null
            ? null
            : Path.Combine(beatmap.Folder, coosu.General.AudioFilename);

        if (audioFilePath == _lastPreviewAudioPath)
        {
            return HandleResult.Continue;
        }

        _lastPreviewAudioPath = audioFilePath;
        _backgroundMusicManager.StopCurrentMusic(200);
        _backgroundMusicManager.PlaySingleAudioPreview(coosu, audioFilePath, coosu.General.PreviewTime);
        _backgroundMusicManager.ResetPauseState();
        
        return HandleResult.Continue;
    }
}
