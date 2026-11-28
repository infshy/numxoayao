namespace VolumeControlApp;

public partial class MainPage : ContentPage
{
	private readonly ViewModels.MainViewModel _viewModel;

	public MainPage(ViewModels.MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
		ApplyLayoutMode();
		_viewModel.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(ViewModels.MainViewModel.IsCompactMode))
				ApplyLayoutMode();
		};
	}

	private void OnSliderDragCompleted(object? sender, EventArgs e) => _viewModel.RegisterGesture();

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		ApplyLayoutMode();
		PageContent.Opacity = 0;
		await PageContent.FadeToAsync(1, 180, Easing.CubicOut);
	}

	private void ApplyLayoutMode() => PageContent.Padding = _viewModel.IsCompactMode
		? new Thickness(8)
		: new Thickness(24, 26);
}
