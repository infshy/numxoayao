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
	private double _lastPointerAngle;
	private bool _hasLastPointerAngle;
	private bool _isDragging;

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
		var target = (double)newValue;
		if (knob._isDragging || !knob.IsLoaded || Math.Abs(target - (double)oldValue) < 0.0001)
		{
			knob._drawable.Value = target;
			knob.KnobGraphicsView.Invalidate();
			return;
		}

		knob.AbortAnimation("knob-value");
		var animation = new Animation(value =>
		{
			knob._drawable.Value = value;
			knob.KnobGraphicsView.Invalidate();
		}, (double)oldValue, target, Easing.CubicOut);
		animation.Commit(knob, "knob-value", 16, 110);
	}

	private void UpdateDrawable()
	{
		_drawable.Value = Value;
	}

	private void OnStartInteraction(object? sender, TouchEventArgs e)
	{
		_isDragging = true;
		_hasLastPointerAngle = TryGetPointerAngle(e, out _lastPointerAngle);
	}

	private void OnDragInteraction(object? sender, TouchEventArgs e)
	{
		if (!TryGetPointerAngle(e, out var currentAngle))
			return;

		if (_hasLastPointerAngle)
		{
			var angleDelta = currentAngle - _lastPointerAngle;
			if (angleDelta > 180d)
				angleDelta -= 360d;
			else if (angleDelta < -180d)
				angleDelta += 360d;

			Value = Math.Clamp(Value + angleDelta / RotaryKnobDrawable.SweepAngle, 0d, 1d);
		}

		_lastPointerAngle = currentAngle;
		_hasLastPointerAngle = true;
	}

	private void OnEndInteraction(object? sender, TouchEventArgs e)
	{
		_isDragging = false;
		_hasLastPointerAngle = false;
	}

	private void OnCancelInteraction(object? sender, EventArgs e)
	{
		_isDragging = false;
		_hasLastPointerAngle = false;
	}

	private bool TryGetPointerAngle(TouchEventArgs e, out double angle)
	{
		angle = 0;
		if (e.Touches.Length == 0)
			return false;

		var touch = e.Touches[0];
		var centerX = (float)(KnobGraphicsView.Width / 2d);
		var centerY = (float)(KnobGraphicsView.Height / 2d);
		if (Math.Abs(touch.X - centerX) < 0.5f && Math.Abs(touch.Y - centerY) < 0.5f)
			return false;

		angle = Math.Atan2(touch.Y - centerY, touch.X - centerX) * 180d / Math.PI;
		return true;
	}
}
