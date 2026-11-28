using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using VolumeControlApp.Services;

namespace VolumeControlApp.Platforms.Windows;

public sealed class WindowsWindowModeService : IWindowModeService
{
    public bool IsSupported => true;

    public void SetCompactMode(bool isCompact)
    {
        var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        if (mauiWindow?.Handler?.PlatformView is not Microsoft.UI.Xaml.Window nativeWindow)
            return;

        var handle = WindowNative.GetWindowHandle(nativeWindow);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        if (appWindow.Presenter is not OverlappedPresenter)
        {
            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        }

        if (appWindow.Presenter is not OverlappedPresenter presenter)
            return;

        presenter.IsAlwaysOnTop = isCompact;
        presenter.IsResizable = true;
        presenter.IsMaximizable = !isCompact;
        appWindow.Resize(isCompact ? new global::Windows.Graphics.SizeInt32(720, 130) : new global::Windows.Graphics.SizeInt32(760, 900));
    }
}
