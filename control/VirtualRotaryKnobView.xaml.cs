using Microsoft.Maui.Controls;

namespace VolumeControlApp.Controls;

public partial class VirtualRotaryKnobView : ContentView
{
	public static readonly BindableProperty ValueProperty = BindableProperty.Create(
		nameof(Value),
		typeof(double),
		typeof(VirtualRotaryKnobView),
		0d,
		BindingMode.TwoWay,
		coerceValue: (_, value) => Math.Clamp((double)value, 0d, 1d),
		propertyChanged: OnValueChanged);

	private readonly RotaryKnobDrawable _drawable = new();
	private double _lastPanAngle;
	private bool _hasLastPanAngle;

	public VirtualRotaryKnobView()
	{
		InitializeComponent();
		KnobGraphicsView.Drawable = _drawable;
		UpdateDrawable();
	}

	public double Value
	{
		get => (double)GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}

	private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var knob = (VirtualRotaryKnobView)bindable;
		knob.UpdateDrawable();
		knob.KnobGraphicsView.Invalidate();
	}

	private void UpdateDrawable()
	{
		_drawable.Value = Value;
	}

	private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
	{
		switch (e.StatusType)
		{
			case GestureStatus.Started:
				_hasLastPanAngle = false;
				break;

			case GestureStatus.Running:
				var deltaX = e.TotalX;
				var deltaY = e.TotalY;
				if (Math.Abs(deltaX) < double.Epsilon && Math.Abs(deltaY) < double.Epsilon)
				{
					return;
				}

				var currentAngle = Math.Atan2(deltaY, deltaX) * 180d / Math.PI;
				if (_hasLastPanAngle)
				{
					var angleDelta = currentAngle - _lastPanAngle;
					if (angleDelta > 180d)
					{
						angleDelta -= 360d;
					}
					else if (angleDelta < -180d)
					{
						angleDelta += 360d;
					}

					Value = Math.Clamp(Value + angleDelta / RotaryKnobDrawable.SweepAngle, 0d, 1d);
				}

				_lastPanAngle = currentAngle;
				_hasLastPanAngle = true;
				break;

			case GestureStatus.Completed:
			case GestureStatus.Canceled:
				_hasLastPanAngle = false;
				break;
		}
	}
}
