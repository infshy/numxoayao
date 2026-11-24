using Microsoft.Maui.Graphics;

namespace VolumeControlApp.Controls;

public sealed class RotaryKnobDrawable : IDrawable
{
	public const float StartAngle = 135f;
	public const float SweepAngle = 270f;

	public double Value { get; set; }

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var width = dirtyRect.Width;
		var height = dirtyRect.Height;
		var size = Math.Min(width, height);
		var centerX = dirtyRect.Center.X;
		var centerY = dirtyRect.Center.Y;
		var arcRadius = size * 0.39f;
		var knobRadius = size * 0.31f;
		var arcBounds = new RectF(
			centerX - arcRadius,
			centerY - arcRadius,
			arcRadius * 2,
			arcRadius * 2);

		var clampedValue = Math.Clamp(Value, 0d, 1d);
		var progressAngle = StartAngle + (float)(SweepAngle * clampedValue);

		canvas.StrokeSize = size * 0.095f;
		canvas.StrokeLineCap = LineCap.Round;
		canvas.StrokeColor = Color.FromArgb("#D9E1E8");
		canvas.DrawArc(arcBounds, StartAngle, StartAngle + SweepAngle, false, false);

		canvas.StrokeColor = Color.FromArgb("#1677FF");
		if (clampedValue > 0)
		{
			canvas.DrawArc(arcBounds, StartAngle, progressAngle, false, false);
		}

		canvas.FillColor = Color.FromArgb("#243447");
		canvas.FillCircle(centerX, centerY, knobRadius);

		canvas.StrokeSize = size * 0.018f;
		canvas.StrokeColor = Color.FromArgb("#52677D");
		canvas.DrawCircle(centerX, centerY, knobRadius);

		var pointerAngle = progressAngle * MathF.PI / 180f;
		var pointerStartRadius = knobRadius * 0.22f;
		var pointerEndRadius = knobRadius * 0.76f;
		var pointerStart = new PointF(
			centerX + MathF.Cos(pointerAngle) * pointerStartRadius,
			centerY + MathF.Sin(pointerAngle) * pointerStartRadius);
		var pointerEnd = new PointF(
			centerX + MathF.Cos(pointerAngle) * pointerEndRadius,
			centerY + MathF.Sin(pointerAngle) * pointerEndRadius);

		canvas.StrokeSize = size * 0.035f;
		canvas.StrokeColor = Colors.White;
		canvas.DrawLine(pointerStart, pointerEnd);
		canvas.FillColor = Colors.White;
		canvas.FillCircle(pointerEnd.X, pointerEnd.Y, size * 0.035f);
	}
}
