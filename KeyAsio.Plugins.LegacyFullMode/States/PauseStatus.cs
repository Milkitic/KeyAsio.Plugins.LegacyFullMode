namespace KeyAsio.Plugins.LegacyFullMode.States;

public class PauseStatus
{
    public bool PreviousSelectSongStatus { get; set; } = true;
    public int PauseCount { get; private set; }
    public void ResetPauseState()
    {
        PreviousSelectSongStatus = true;
        PauseCount = 0;
    }

    public void UpdatePauseCount(bool paused)
    {
        if (paused && PreviousSelectSongStatus)
        {
            PauseCount++;
        }
        else if (!paused)
        {
            PauseCount = 0;
        }
    }
}