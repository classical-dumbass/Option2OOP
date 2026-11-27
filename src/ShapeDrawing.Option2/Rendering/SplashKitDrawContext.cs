using System.Numerics;
using ShapeDrawing.Option2.Rendering;
using SplashKitSDK;
using ModelColor = System.Drawing.Color;

namespace ShapeDrawing.Option2.Rendering;

public sealed class SplashKitDrawContext : IDrawContext
{
    public void DrawLine(Vector2 start, Vector2 end, ModelColor color, float thickness)
    {
        var splashColor = ToSplashKitColor(color);
        var direction = end - start;
        var length = direction.Length();
        var normal = length <= float.Epsilon
            ? Vector2.UnitX
            : new Vector2(-direction.Y / length, direction.X / length);
        var lineCount = Math.Max(1, (int)MathF.Ceiling(thickness));

        for (var index = 0; index < lineCount; index++)
        {
            var offset = normal * (index - ((lineCount - 1) / 2f));
            var lineStart = start + offset;
            var lineEnd = end + offset;
            SplashKit.DrawLine(
                splashColor,
                lineStart.X,
                lineStart.Y,
                lineEnd.X,
                lineEnd.Y);
        }
    }

    public void DrawRectangle(
        Vector2 center,
        float width,
        float height,
        ModelColor color,
        float rotationDegrees)
    {
        var halfWidth = width / 2f;
        var halfHeight = height / 2f;
        var corners = new[]
        {
            new Vector2(center.X - halfWidth, center.Y - halfHeight),
            new Vector2(center.X + halfWidth, center.Y - halfHeight),
            new Vector2(center.X + halfWidth, center.Y + halfHeight),
            new Vector2(center.X - halfWidth, center.Y + halfHeight)
        };

        for (var index = 0; index < corners.Length; index++)
        {
            var start = RotatePoint(corners[index], center, rotationDegrees);
            var end = RotatePoint(corners[(index + 1) % corners.Length], center, rotationDegrees);
            DrawLine(start, end, color, 1f);
        }
    }

    public void DrawCircle(Vector2 center, float radius, ModelColor color)
    {
        SplashKit.DrawCircle(ToSplashKitColor(color), center.X, center.Y, radius);
    }

    private static Vector2 RotatePoint(Vector2 point, Vector2 center, float degrees)
    {
        var radians = degrees * (MathF.PI / 180f);
        var offset = point - center;
        var cosine = MathF.Cos(radians);
        var sine = MathF.Sin(radians);
        return center + new Vector2(
            (offset.X * cosine) - (offset.Y * sine),
            (offset.X * sine) + (offset.Y * cosine));
    }

    private static SplashKitSDK.Color ToSplashKitColor(ModelColor color)
    {
        return SplashKit.RGBColor(color.R, color.G, color.B);
    }
}
