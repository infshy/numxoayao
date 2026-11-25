using System.Diagnostics;

namespace VolumeControlApp.ViewModels;

public partial class MainViewModel
{
    private double _volumeBeforeMute;
    private long _controlStartedAt;
    private int _controlChangeDepth;

    partial void OnIsMutedChanging(bool oldValue, bool newValue) => BeginControlTiming();

    partial void OnIsMutedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            _volumeBeforeMute = Volume;
            Volume = 0;
        }
        else if (Volume == 0)
        {
            // Nếu người dùng kéo thanh trượt để bật tiếng, Volume đã có mức mới.
            Volume = _volumeBeforeMute;
        }

        FinishControlTiming();
    }

    partial void OnVolumeChanging(double oldValue, double newValue) => BeginControlTiming();

    partial void OnVolumeChanged(double oldValue, double newValue) => FinishControlTiming();

    private void BeginControlTiming()
    {
        // Tắt tiếng và làm tròn âm lượng có thể gọi lồng nhau trong cùng một thao tác.
        if (_controlChangeDepth == 0)
            _controlStartedAt = Stopwatch.GetTimestamp();

        _controlChangeDepth++;
    }

    private void FinishControlTiming()
    {
        _controlChangeDepth--;
        if (_controlChangeDepth == 0)
            TouchLatencyMs = Stopwatch.GetElapsedTime(_controlStartedAt).TotalMilliseconds;
    }
}
