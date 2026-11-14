using VolumeControl.ViewModels;

namespace VolumeControl.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _vm;

    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    private void OnSliderDragCompleted(object? sender, EventArgs e) => _vm.RegisterGesture();
}
