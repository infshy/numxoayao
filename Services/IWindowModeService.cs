namespace VolumeControlApp.Services;

public interface IWindowModeService
{
    bool IsSupported { get; }

    void SetCompactMode(bool isCompact);
}

public sealed class UnsupportedWindowModeService : IWindowModeService
{
    public bool IsSupported => false;
    public void SetCompactMode(bool isCompact) { }
}
