using Plugin.Maui.Audio;

namespace VolumeControlApp.Services;

public sealed class AudioPlaybackService(IAudioManager audioManager) : IAudioPlaybackService
{
    private const string SampleFileName = "sample.wav";
    private readonly SemaphoreSlim _playerLock = new(1, 1);
    private IAudioPlayer? _player;
    private Stream? _audioStream;
    private double _volume = 0.5;
    private bool _isMuted;
    private bool _playbackEnded;
    private bool _disposed;

    public event EventHandler? PlaybackStateChanged;

    public bool IsPlaying => !_disposed && _player?.IsPlaying == true;

    public async Task PlayAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _playerLock.WaitAsync();
        try
        {
            if (_player is null)
            {
                var stream = await FileSystem.OpenAppPackageFileAsync(SampleFileName);
                try
                {
                    _player = audioManager.CreatePlayer(stream);
                    _audioStream = stream;
                }
                catch
                {
                    await stream.DisposeAsync();
                    throw;
                }

                _player.PlaybackEnded += OnPlaybackEnded;
            }

            if (_playbackEnded && _player.CanSeek)
                _player.Seek(0);

            _playbackEnded = false;
            ApplyVolume();
            _player.Play();
            PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _playerLock.Release();
        }
    }

    public void Pause()
    {
        if (_player?.IsPlaying != true)
            return;

        _player.Pause();
        PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetVolume(double volume)
    {
        _volume = Math.Clamp(volume, 0d, 1d);
        ApplyVolume();
    }

    public void SetMuted(bool isMuted)
    {
        _isMuted = isMuted;
        ApplyVolume();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_player is not null)
        {
            _player.PlaybackEnded -= OnPlaybackEnded;
            _player.Dispose();
            _player = null;
        }

        _audioStream?.Dispose();
        _audioStream = null;
        PlaybackStateChanged = null;
        _playerLock.Dispose();
    }

    private void ApplyVolume()
    {
        if (_player is not null)
            _player.Volume = _isMuted ? 0d : _volume;
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        _playbackEnded = true;
        PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
