using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
#if WINDOWS
using VolumeControlApp.Platforms.Windows;
#endif
using VolumeControlApp.Services;
using VolumeControlApp.ViewModels;

namespace VolumeControlApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.AddAudio()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<IAudioPlaybackService, AudioPlaybackService>();
#if WINDOWS
		builder.Services.AddSingleton<ISystemVolumeService, WindowsSystemVolumeService>();
		builder.Services.AddSingleton<IWindowModeService, WindowsWindowModeService>();
#else
		builder.Services.AddSingleton<ISystemVolumeService, UnsupportedSystemVolumeService>();
		builder.Services.AddSingleton<IWindowModeService, UnsupportedWindowModeService>();
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
