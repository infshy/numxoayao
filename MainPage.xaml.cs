namespace VolumeControlApp;

public partial class MainPage : ContentPage
{
	private readonly ViewModels.MainViewModel _viewModel;

	public MainPage(ViewModels.MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	private void OnSliderDragCompleted(object? sender, EventArgs e) => _viewModel.RegisterGesture();
}
