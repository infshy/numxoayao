using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using VolumeControlApp.Services;

namespace VolumeControlApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAudioPlaybackService _audioPlaybackService;
    private readonly Stopwatch _latency = new();

    public MainViewModel(IAudioPlaybackService audioPlaybackService)
    {
        _audioPlaybackService = audioPlaybackService;
        Volume = Math.Clamp(Preferences.Default.Get("volume", 0.5d), 0d, 1d);
        UseKnob = Preferences.Default.Get("useKnob", true);
        IsDarkTheme = Preferences.Default.Get("darkTheme", true);
        ApplyTheme();
        _audioPlaybackService.SetVolume(Volume);
        _audioPlaybackService.PlaybackStateChanged += OnPlaybackStateChanged;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VolumePercent), nameof(VolumeText))]
    private double _volume = 0.5;

    [ObservableProperty] private bool _isMuted;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _useKnob = true;
    [ObservableProperty] private bool _isDarkTheme = true;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudioStatusText))]
    private string? _audioError;
    [ObservableProperty] private double _touchLatencyMs;
    [ObservableProperty] private int _gestureCount;

    public int VolumePercent => (int)Math.Round(Volume * 100);
    public string VolumeText => IsMuted ? "MUTE" : $"{VolumePercent}%";
    public string MuteButtonText => IsMuted ? "Bật tiếng" : "Tắt tiếng";
    public string PlayButtonText => IsPlaying ? "Tạm dừng" : "Phát";
    public string ModeText => UseKnob ? "Núm xoay ảo" : "Thanh trượt";
    public string ThemeButtonText => IsDarkTheme ? "☀  Giao diện sáng" : "☾  Giao diện tối";
    public string AudioStatusText => AudioError ?? (IsPlaying ? "Đang phát âm thanh mẫu" : "Sẵn sàng phát âm thanh mẫu");
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

        _audioPlaybackService.SetVolume(snapped);
        Preferences.Default.Set("volume", snapped);
        _latency.Stop();
        TouchLatencyMs = Math.Round(_latency.Elapsed.TotalMilliseconds, 2);
    }

    partial void OnUseKnobChanged(bool value)
    {
        OnPropertyChanged(nameof(ModeText));
        OnPropertyChanged(nameof(UseSlider));
        Preferences.Default.Set("useKnob", value);
    }
    partial void OnIsPlayingChanged(bool value)
    {
        OnPropertyChanged(nameof(PlayButtonText));
        OnPropertyChanged(nameof(AudioStatusText));
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        ApplyTheme();
        OnPropertyChanged(nameof(ThemeButtonText));
        Preferences.Default.Set("darkTheme", value);
    }

    partial void OnIsMutedChanged(bool value)
    {
        _audioPlaybackService.SetMuted(value);
        OnPropertyChanged(nameof(VolumeText));
        OnPropertyChanged(nameof(MuteButtonText));
    }

    [RelayCommand] private void ToggleMute() => IsMuted = !IsMuted;
    [RelayCommand]
    private async Task TogglePlay()
    {
        if (IsPlaying)
        {
            _audioPlaybackService.Pause();
            IsPlaying = false;
            return;
        }

        try
        {
            AudioError = null;
            await _audioPlaybackService.PlayAsync();
            IsPlaying = _audioPlaybackService.IsPlaying;
        }
        catch (Exception)
        {
            IsPlaying = false;
            AudioError = "Không thể phát âm thanh mẫu.";
        }
    }
    [RelayCommand] private void ToggleMode() => UseKnob = !UseKnob;
    [RelayCommand] private void ToggleTheme() => IsDarkTheme = !IsDarkTheme;
    [RelayCommand] private void Step(string delta) => Volume = Math.Clamp(
        Volume + double.Parse(delta, CultureInfo.InvariantCulture) / 100d, 0, 1);

    public void RegisterGesture() => GestureCount++;

    private void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => IsPlaying = _audioPlaybackService.IsPlaying);
    }

    private void ApplyTheme()
    {
        if (Application.Current is { } app)
            app.UserAppTheme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
    }
}
