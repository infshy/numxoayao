namespace VolumeControlApp;

public partial class App : Application
{
	private readonly MainPage _mainPage;
	private readonly Services.IAudioPlaybackService _audioPlaybackService;
	private readonly Services.ISystemVolumeService _systemVolumeService;

	public App(MainPage mainPage, Services.IAudioPlaybackService audioPlaybackService, Services.ISystemVolumeService systemVolumeService)
	{
		InitializeComponent();
		_mainPage = mainPage;
		_audioPlaybackService = audioPlaybackService;
		_systemVolumeService = systemVolumeService;
		if (mainPage.BindingContext is ViewModels.MainViewModel viewModel)
			UserAppTheme = viewModel.IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(_mainPage) { Title = "Volume Control", Width = 700, Height = 860 };
		window.Destroying += (_, _) =>
		{
			_audioPlaybackService.Dispose();
			_systemVolumeService.Dispose();
		};
		return window;
	}
}
