namespace VolumeControlApp.Services;

public sealed record AudioOutputDevice(string? Id, string Name, bool IsDefault = false);

public interface ISystemVolumeService : IDisposable
{
    event EventHandler? VolumeChanged;

    bool IsAvailable { get; }

    double Volume { get; }

    bool IsMuted { get; }

    string? SelectedDeviceId { get; }

    IReadOnlyList<AudioOutputDevice> GetOutputDevices();

    void SelectDevice(string? deviceId);

    void SetVolume(double volume);

    void SetMuted(bool isMuted);
}

public sealed class UnsupportedSystemVolumeService : ISystemVolumeService
{
    public event EventHandler? VolumeChanged { add { } remove { } }
    public bool IsAvailable => false;
    public double Volume => 0.5d;
    public bool IsMuted => false;
    public string? SelectedDeviceId => null;
    public IReadOnlyList<AudioOutputDevice> GetOutputDevices() => Array.Empty<AudioOutputDevice>();
    public void SelectDevice(string? deviceId) { }
    public void SetVolume(double volume) { }
    public void SetMuted(bool isMuted) { }
    public void Dispose() { }
}
