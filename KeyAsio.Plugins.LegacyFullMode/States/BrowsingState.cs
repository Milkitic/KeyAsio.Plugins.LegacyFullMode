using Coosu.Beatmap;
using KeyAsio.Plugins.Abstractions;
using KeyAsio.Shared;
using KeyAsio.Shared.Sync.Services;

namespace KeyAsio.Plugins.LegacyFullMode.States;

public class BrowsingState : IGameStateHandler
{
    private readonly AppSettings _appSettings;
    private readonly BackgroundMusicManager _backgroundMusicManager;
    private readonly PauseStatus _pauseStatus;

    private string? _lastPreviewAudioPath;
    private int _previewAudioTime = int.MinValue;

    public BrowsingState(AppSettings appSettings,
        BackgroundMusicManager backgroundMusicManager,
        PauseStatus pauseStatus)
    {
        _appSettings = appSettings;
        _backgroundMusicManager = backgroundMusicManager;
        _pauseStatus = pauseStatus;
    }

    public int Priority => 10;

    public HandleResult HandleEnter(ISyncContext context)
    {
        _backgroundMusicManager.StartLowPass(200, 16000);
        return HandleResult.Continue;
    }

    public HandleResult HandleExit(ISyncContext context)
    {
        return HandleResult.Continue;
    }

    public HandleResult HandleTick(ISyncContext context)
    {
        const int selectSongPauseThreshold = 20;
        if (!_appSettings.Sync.EnableMixSync) return HandleResult.Continue;

        // Maintain pause state lifecycle for song-select preview
        _pauseStatus.UpdatePauseCount(_previewAudioTime == context.PlayTime);

        if (_pauseStatus.PauseCount >= selectSongPauseThreshold &&
            _pauseStatus.PreviousSelectSongStatus)
        {
            _backgroundMusicManager.PauseCurrentMusic();
            _pauseStatus.PreviousSelectSongStatus = false;
        }
        else if (_pauseStatus.PauseCount < selectSongPauseThreshold &&
                 !_pauseStatus.PreviousSelectSongStatus)
        {
            _backgroundMusicManager.RecoverCurrentMusic();
            _pauseStatus.PreviousSelectSongStatus = true;
        }

        _previewAudioTime = context.PlayTime;
        return HandleResult.Continue;
    }

    public HandleResult HandleBeatmapChange(SyncBeatmapInfo beatmap)
    {
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
        _pauseStatus.ResetPauseState();

        return HandleResult.Continue;
    }
}