namespace VolumeControlApp.Services;

public interface IAudioPlaybackService : IDisposable
{
    event EventHandler? PlaybackStateChanged;

    bool IsPlaying { get; }

    Task PlayAsync();

    void Pause();

    void SetVolume(double volume);

    void SetMuted(bool isMuted);
}
