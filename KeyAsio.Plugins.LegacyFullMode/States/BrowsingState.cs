using Coosu.Beatmap;
using KeyAsio.Plugins.Abstractions;
using KeyAsio.Plugins.LegacyFullMode.Tracks;
using KeyAsio.Shared;

namespace KeyAsio.Plugins.LegacyFullMode.States;

public class BrowsingState : IGameStateHandler
{
    private readonly AppSettings _appSettings;
    private readonly SongPreviewPlayer _songPreviewPlayer;
    private readonly PauseStatus _pauseStatus;

    private string? _lastPreviewAudioPath;
    private int _previewAudioTime = int.MinValue;

    public BrowsingState(AppSettings appSettings,
        SongPreviewPlayer songPreviewPlayer,
        PauseStatus pauseStatus)
    {
        _appSettings = appSettings;
        _songPreviewPlayer = songPreviewPlayer;
        _pauseStatus = pauseStatus;
    }

    public int Priority => 10;

    public HandleResult HandleEnter(ISyncContext context)
    {
        _songPreviewPlayer.StartLowPass(200, 16000);
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
            _ = _songPreviewPlayer.PauseCurrentMusic();
            _pauseStatus.PreviousSelectSongStatus = false;
        }
        else if (_pauseStatus.PauseCount < selectSongPauseThreshold &&
                 !_pauseStatus.PreviousSelectSongStatus)
        {
            _ = _songPreviewPlayer.RecoverCurrentMusic();
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
        _ = _songPreviewPlayer.StopCurrentMusic(200);
        _ = _songPreviewPlayer.Play(coosu, audioFilePath, coosu.General.PreviewTime);
        _pauseStatus.ResetPauseState();

        return HandleResult.Continue;
    }
}