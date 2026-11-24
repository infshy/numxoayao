using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VolumeControlApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Stopwatch _latency = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VolumePercent), nameof(VolumeText))]
    private double _volume = 0.5;

    [ObservableProperty] private bool _isMuted;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _useKnob = true;
    [ObservableProperty] private double _touchLatencyMs;
    [ObservableProperty] private int _gestureCount;

    public int VolumePercent => (int)Math.Round(Volume * 100);
    public string VolumeText => IsMuted ? "MUTE" : $"{VolumePercent}%";
    public string MuteButtonText => IsMuted ? "Bật tiếng" : "Tắt tiếng";
    public string PlayButtonText => IsPlaying ? "Dừng" : "Phát";
    public string ModeText => UseKnob ? "Núm xoay ảo" : "Thanh trượt";
    public bool UseSlider => !UseKnob;

    partial void OnVolumeChanging(double value) => _latency.Restart();

    partial void OnVolumeChanged(double value)
    {
        var snapped = Math.Round(Math.Clamp(value, 0, 1), 2);
        if (Math.Abs(snapped - value) > 1e-9)
        {
            Volume = snapped;
            return;
        }

        _latency.Stop();
        TouchLatencyMs = Math.Round(_latency.Elapsed.TotalMilliseconds, 2);
        if (IsMuted && snapped > 0)
            IsMuted = false;
    }

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
    [RelayCommand] private void Step(string delta) => Volume = Math.Clamp(
        Volume + double.Parse(delta, CultureInfo.InvariantCulture) / 100d, 0, 1);

    public void RegisterGesture() => GestureCount++;
}
