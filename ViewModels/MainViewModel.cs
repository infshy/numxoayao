using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using VolumeControlApp.Services;
using System.Collections.ObjectModel;

namespace VolumeControlApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAudioPlaybackService _audioPlaybackService;
    private readonly ISystemVolumeService _systemVolumeService;
    private readonly IWindowModeService _windowModeService;
    private readonly Stopwatch _latency = new();
    private bool _syncingFromSystem;

    public MainViewModel(
        IAudioPlaybackService audioPlaybackService,
        ISystemVolumeService systemVolumeService,
        IWindowModeService windowModeService)
    {
        _audioPlaybackService = audioPlaybackService;
        _systemVolumeService = systemVolumeService;
        _windowModeService = windowModeService;
        IsSystemVolumeAvailable = _systemVolumeService.IsAvailable;
        OutputDevices = new ObservableCollection<AudioOutputDevice>();
        if (IsSystemVolumeAvailable)
        {
            OutputDevices.Add(new AudioOutputDevice(null, "Thiết bị mặc định", true));
            foreach (var device in _systemVolumeService.GetOutputDevices())
                OutputDevices.Add(device with { Name = device.IsDefault ? $"Mặc định · {device.Name}" : device.Name });

            SelectedOutputDevice = OutputDevices.FirstOrDefault(device =>
                string.Equals(device.Id, _systemVolumeService.SelectedDeviceId, StringComparison.OrdinalIgnoreCase))
                ?? OutputDevices[0];
        }

        Volume = Math.Clamp(
            IsSystemVolumeAvailable ? _systemVolumeService.Volume : Preferences.Default.Get("volume", 0.5d), 0d, 1d);
        IsMuted = IsSystemVolumeAvailable && _systemVolumeService.IsMuted;
        UseKnob = Preferences.Default.Get("useKnob", true);
        IsDarkTheme = Preferences.Default.Get("darkTheme", true);
        ApplyTheme();
        _audioPlaybackService.SetVolume(Volume);
        _audioPlaybackService.SetMuted(IsMuted);
        _audioPlaybackService.PlaybackStateChanged += OnPlaybackStateChanged;
        _systemVolumeService.VolumeChanged += OnSystemVolumeChanged;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VolumePercent), nameof(VolumeText))]
    private double _volume = 0.5;

    [ObservableProperty] private bool _isMuted;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _useKnob = true;
    [ObservableProperty] private bool _isDarkTheme = true;
    [ObservableProperty] private bool _isSettingsVisible;
    [ObservableProperty] private bool _isCompactMode;
    [ObservableProperty] private AudioOutputDevice? _selectedOutputDevice;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudioStatusText))]
    private string? _audioError;
    [ObservableProperty] private double _touchLatencyMs;
    [ObservableProperty] private int _gestureCount;

    public int VolumePercent => (int)Math.Round(Volume * 100);
    public string VolumeText => IsMuted ? "MUTE" : $"{VolumePercent}%";
    public string MuteButtonText => IsMuted ? "UNMUTE" : "MUTE";
    public string PlayButtonText => IsPlaying ? "Ⅱ  TẠM DỪNG" : "▶  PHÁT MẪU";
    public Color MuteButtonBackground => IsMuted ? Color.FromArgb("#B65367") : Color.FromArgb("#22252C");
    public Color PlaybackButtonBackground => IsPlaying ? Color.FromArgb("#353147") : Color.FromArgb("#8B7CFF");
    public string ModeText => UseKnob ? "NÚM XOAY + SLIDER" : "CHỈ SLIDER";
    public string ThemeButtonText => IsDarkTheme ? "Sáng" : "Tối";
    public string SystemVolumeStatusText => IsSystemVolumeAvailable ? "SYSTEM OUTPUT" : "APP AUDIO";
    public string VolumeTargetText => IsSystemVolumeAvailable
        ? $"THIẾT BỊ CẦN ĐIỀU KHIỂN · {SelectedOutputDevice?.Name ?? "Mặc định"}"
        : "IN-APP AUDIO PLAYER";
    public string SettingsButtonText => IsSettingsVisible ? "Đóng" : "Cài đặt";
    public string AudioStatusText => AudioError ?? (IsPlaying ? "ĐANG PHÁT" : "SẴN SÀNG");
    public bool UseSlider => !UseKnob;
    public bool IsStandardMode => !IsCompactMode;
    public bool IsCompactModeSupported => _windowModeService.IsSupported;
    public string CompactButtonText => IsCompactMode ? "MỞ RỘNG" : "MINI";
    public bool IsSystemVolumeAvailable { get; }
    public ObservableCollection<AudioOutputDevice> OutputDevices { get; }

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
        if (IsSystemVolumeAvailable && !_syncingFromSystem)
            _systemVolumeService.SetVolume(snapped);
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
        OnPropertyChanged(nameof(PlaybackButtonBackground));
        OnPropertyChanged(nameof(AudioStatusText));
    }

    partial void OnIsMutedChanged(bool value)
    {
        _audioPlaybackService.SetMuted(value);
        if (IsSystemVolumeAvailable && !_syncingFromSystem)
            _systemVolumeService.SetMuted(value);
        OnPropertyChanged(nameof(VolumeText));
        OnPropertyChanged(nameof(MuteButtonText));
        OnPropertyChanged(nameof(MuteButtonBackground));
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        ApplyTheme();
        OnPropertyChanged(nameof(ThemeButtonText));
        Preferences.Default.Set("darkTheme", value);
    }

    partial void OnIsSettingsVisibleChanged(bool value) => OnPropertyChanged(nameof(SettingsButtonText));

    partial void OnIsCompactModeChanged(bool value)
    {
        _windowModeService.SetCompactMode(value);
        OnPropertyChanged(nameof(IsStandardMode));
        OnPropertyChanged(nameof(CompactButtonText));
    }

    partial void OnSelectedOutputDeviceChanged(AudioOutputDevice? value)
    {
        if (!IsSystemVolumeAvailable || value is null)
            return;

        _systemVolumeService.SelectDevice(value.Id);
        OnPropertyChanged(nameof(VolumeTargetText));
        OnSystemVolumeChanged(this, EventArgs.Empty);
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
    [RelayCommand] private void ToggleSettings() => IsSettingsVisible = !IsSettingsVisible;
    [RelayCommand] private void ToggleCompactMode() => IsCompactMode = !IsCompactMode;
    [RelayCommand] private void Step(string delta) => Volume = Math.Clamp(
        Volume + double.Parse(delta, CultureInfo.InvariantCulture) / 100d, 0, 1);

    public void RegisterGesture() => GestureCount++;

    private void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => IsPlaying = _audioPlaybackService.IsPlaying);
    }

    private void OnSystemVolumeChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!IsSystemVolumeAvailable)
                return;

            _syncingFromSystem = true;
            try
            {
                Volume = Math.Clamp(_systemVolumeService.Volume, 0d, 1d);
                IsMuted = _systemVolumeService.IsMuted;
            }
            finally
            {
                _syncingFromSystem = false;
            }
        });
    }

    private void ApplyTheme()
    {
        if (Application.Current is { } app)
            app.UserAppTheme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
    }
}
