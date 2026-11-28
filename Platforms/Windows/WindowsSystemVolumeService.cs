using NAudio.CoreAudioApi;
using Microsoft.Maui.Storage;
using VolumeControlApp.Services;

namespace VolumeControlApp.Platforms.Windows;

public sealed class WindowsSystemVolumeService : ISystemVolumeService
{
    private MMDeviceEnumerator? _enumerator;
    private MMDevice? _device;
    private AudioEndpointVolume? _endpointVolume;
    private readonly AudioEndpointVolumeNotificationDelegate _volumeNotificationHandler;
    private string? _selectedDeviceId;
    private bool _disposed;

    public WindowsSystemVolumeService()
    {
        _volumeNotificationHandler = _ => VolumeChanged?.Invoke(this, EventArgs.Empty);
        var savedDeviceId = Preferences.Default.Get("outputDeviceId", string.Empty);
        _selectedDeviceId = string.IsNullOrWhiteSpace(savedDeviceId) ? null : savedDeviceId;
        RefreshSelectedDevice();
    }

    public event EventHandler? VolumeChanged;

    public bool IsAvailable => !_disposed && _device is not null;

    public double Volume => IsAvailable ? _endpointVolume!.MasterVolumeLevelScalar : 0.5d;

    public bool IsMuted => IsAvailable && _endpointVolume!.Mute;

    public string? SelectedDeviceId => _selectedDeviceId;

    public IReadOnlyList<AudioOutputDevice> GetOutputDevices()
    {
        if (_disposed)
            return Array.Empty<AudioOutputDevice>();

        try
        {
            _enumerator ??= new MMDeviceEnumerator();
            using var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var devices = new List<AudioOutputDevice>();
            foreach (var endpoint in _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                try
                {
                    devices.Add(new AudioOutputDevice(
                        endpoint.ID,
                        endpoint.FriendlyName,
                        string.Equals(endpoint.ID, defaultDevice.ID, StringComparison.OrdinalIgnoreCase)));
                }
                finally
                {
                    endpoint.Dispose();
                }
            }

            return devices;
        }
        catch
        {
            return Array.Empty<AudioOutputDevice>();
        }
    }

    public void SelectDevice(string? deviceId)
    {
        if (_disposed || string.Equals(_selectedDeviceId, deviceId, StringComparison.OrdinalIgnoreCase))
            return;

        _selectedDeviceId = string.IsNullOrWhiteSpace(deviceId) ? null : deviceId;
        Preferences.Default.Set("outputDeviceId", _selectedDeviceId ?? string.Empty);
        RefreshSelectedDevice();
        VolumeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetVolume(double volume)
    {
        if (_disposed)
            return;

        if (_device is null)
            RefreshSelectedDevice();

        if (_endpointVolume is not null)
        {
            var clamped = (float)Math.Clamp(volume, 0d, 1d);
            if (Math.Abs(_endpointVolume.MasterVolumeLevelScalar - clamped) > 0.002f)
                _endpointVolume.MasterVolumeLevelScalar = clamped;
        }
    }

    public void SetMuted(bool isMuted)
    {
        if (_disposed)
            return;

        if (_device is null)
            RefreshSelectedDevice();

        if (_endpointVolume is not null && _endpointVolume.Mute != isMuted)
            _endpointVolume.Mute = isMuted;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_endpointVolume is not null)
        {
            _endpointVolume.OnVolumeNotification -= _volumeNotificationHandler;
            _endpointVolume.Dispose();
        }
        _device?.Dispose();
        _enumerator?.Dispose();
        _endpointVolume = null;
        _device = null;
        _enumerator = null;
        VolumeChanged = null;
    }

    private void RefreshSelectedDevice()
    {
        if (_disposed)
            return;

        try
        {
            _enumerator ??= new MMDeviceEnumerator();
            MMDevice? defaultDevice = null;
            if (_selectedDeviceId is not null)
            {
                try
                {
                    var selectedDevice = _enumerator.GetDevice(_selectedDeviceId);
                    if (selectedDevice.State == DeviceState.Active)
                        defaultDevice = selectedDevice;
                    else
                        selectedDevice.Dispose();
                }
                catch
                {
                    _selectedDeviceId = null;
                    Preferences.Default.Set("outputDeviceId", string.Empty);
                }
            }

            defaultDevice ??= _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (string.Equals(defaultDevice.ID, _device?.ID, StringComparison.OrdinalIgnoreCase))
            {
                defaultDevice.Dispose();
                return;
            }

            if (_endpointVolume is not null)
            {
                _endpointVolume.OnVolumeNotification -= _volumeNotificationHandler;
                _endpointVolume.Dispose();
            }
            _device?.Dispose();
            _device = defaultDevice;
            _endpointVolume = _device.AudioEndpointVolume;
            _endpointVolume.OnVolumeNotification += _volumeNotificationHandler;
        }
        catch
        {
            if (_endpointVolume is not null)
            {
                _endpointVolume.OnVolumeNotification -= _volumeNotificationHandler;
                _endpointVolume.Dispose();
                _endpointVolume = null;
            }
            _device?.Dispose();
            _device = null;
        }
    }
}
