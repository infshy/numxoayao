using VolumeControlApp.ViewModels;

namespace VolumeControlApp;

public partial class MainPage
{
    private bool _hasLatencyLabel;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLatencyLabel || Content is not ScrollView { Content: VerticalStackLayout pageLayout })
            return;

        var latencyLabel = new Label
        {
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Colors.White
        };
        latencyLabel.SetBinding(Label.TextProperty, new Binding(
            nameof(MainViewModel.TouchLatencyMs), stringFormat: "Độ trễ xử lý {0:F3} ms"));
        pageLayout.Children.Add(latencyLabel);
        _hasLatencyLabel = true;
    }
}
