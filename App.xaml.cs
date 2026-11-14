using VolumeControl.Views;

namespace VolumeControl;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(_services.GetRequiredService<MainPage>()) { Title = "Volume Control", Width = 700, Height = 860 };
}
