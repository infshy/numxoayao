using System.Diagnostics;

namespace VolumeControl.ViewModels;

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
            // Unmute bằng nút. Nếu Slider vừa chọn mức dương thì giữ mức vừa chọn.
            Volume = _volumeBeforeMute;
        }

        FinishControlTiming();
    }

    partial void OnVolumeChanging(double oldValue, double newValue) => BeginControlTiming();

    partial void OnVolumeChanged(double oldValue, double newValue) => FinishControlTiming();

    private void BeginControlTiming()
    {
        // Một lần Mute có thể đổi cả IsMuted và Volume. Làm tròn Slider cũng có thể đổi Volume lần nữa.
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
