using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VolumeControl.ViewModels;

/// <summary>
/// ViewModel dùng chung cho Linear Slider, Rotary Knob (Hùng) và Audio (Giang).
/// Volume: 0.0 - 1.0, bước 1%.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly Stopwatch _latency = new();
    private double _lastVolume;

    // Giá trị chung 0.0 - 1.0 (Slider và Knob cùng bind vào đây - two-way)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VolumePercent), nameof(VolumeText), nameof(KnobAngle), nameof(SliderTravel))]
    private double _volume = 0.5;

    [ObservableProperty] private bool _isMuted;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _useKnob = true;   // UI toggle: Knob <-> Slider

    // --- Metrics thời gian thực ---
    [ObservableProperty] private double _touchLatencyMs;
    [ObservableProperty] private int _gestureCount;
    [ObservableProperty] private double _taskTimeSec;
    [ObservableProperty] private double _accuracyError;
    [ObservableProperty] private int _fps = 60;

    public const double KnobMaxAngle = 270;   // độ
    public const double SliderLength = 280;   // dp

    public int VolumePercent => (int)Math.Round(Volume * 100);
    public string VolumeText => IsMuted ? "MUTE" : $"{VolumePercent}%";
    public double KnobAngle => Volume * KnobMaxAngle;
    public double SliderTravel => Volume * SliderLength;

    public bool UseSlider => !UseKnob;
    public string MuteButtonText => IsMuted ? "Unmute" : "Mute";
    public string PlayButtonText => IsPlaying ? "Tạm dừng" : "Phát";
    public string ModeText => UseKnob ? "Núm xoay ảo" : "Thanh trượt";

    partial void OnVolumeChanging(double value)
    {
        _latency.Restart();
    }

    partial void OnVolumeChanged(double value)
    {
        // Làm tròn bước 1%
        var snapped = Math.Round(value, 2);
        if (Math.Abs(snapped - value) > 1e-9) { Volume = snapped; return; }

        _latency.Stop();
        TouchLatencyMs = Math.Round(_latency.Elapsed.TotalMilliseconds, 2);
        _lastVolume = snapped;
        if (IsMuted && snapped > 0) IsMuted = false;
        OnVolumeApplied(snapped);   // Giang: gắn IAudioPlayer.Volume tại đây
    }

    /// <summary>Hook cho Giang (Audio): cập nhật âm lượng thực tế.</summary>
    partial void OnVolumeApplied(double volume);

    partial void OnUseKnobChanged(bool value)
    {
        OnPropertyChanged(nameof(ModeText));
        OnPropertyChanged(nameof(UseSlider));
    }
    partial void OnIsPlayingChanged(bool value) => OnPropertyChanged(nameof(PlayButtonText));
    partial void OnIsMutedChanged(bool value)
    {
        OnPropertyChanged(nameof(VolumeText));
        OnPropertyChanged(nameof(MuteButtonText));
    }

    [RelayCommand] private void ToggleMute() => IsMuted = !IsMuted;
    [RelayCommand] private void TogglePlay() => IsPlaying = !IsPlaying;
    [RelayCommand] private void ToggleMode() => UseKnob = !UseKnob;
    [RelayCommand] private void Step(string delta) => Volume = Math.Clamp(Volume + double.Parse(delta, System.Globalization.CultureInfo.InvariantCulture) / 100.0, 0, 1);

    [RelayCommand]
    private void ResetMetrics()
    {
        GestureCount = 0; TouchLatencyMs = 0; TaskTimeSec = 0; AccuracyError = 0;
    }

    /// <summary>View gọi mỗi khi người dùng nhả tay (kết thúc 1 gesture).</summary>
    public void RegisterGesture() => GestureCount++;
}
