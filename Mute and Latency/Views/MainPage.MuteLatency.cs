using VolumeControl.ViewModels;

namespace VolumeControl.Views;

public partial class MainPage
{
    private Label? _latencyLabel;

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (_latencyLabel is not null || BindingContext is not MainViewModel ||
            VolumeSlider?.Parent is not VerticalStackLayout controls)
            return;

        _latencyLabel = new Label
        {
            HorizontalTextAlignment = TextAlignment.Center
        };
        _latencyLabel.SetDynamicResource(VisualElement.StyleProperty, "MetricName");
        _latencyLabel.SetBinding(Label.TextProperty, new Binding(
            nameof(MainViewModel.TouchLatencyMs), stringFormat: "Độ trễ xử lý {0:F3} ms"));
        controls.Children.Add(_latencyLabel);
    }
}
