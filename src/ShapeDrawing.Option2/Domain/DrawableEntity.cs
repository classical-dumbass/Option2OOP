using System.Drawing;
using System.Numerics;
using ShapeDrawing.Option2.Rendering;

namespace ShapeDrawing.Option2.Domain;

public abstract class DrawableEntity
{
    private Vector2 _center;
    private Color _color;
    private float _rotationDegrees;
    private float _scale = 1f;

    protected DrawableEntity(Vector2 center, Color color)
    {
        _center = center;
        _color = color;
    }

    public Vector2 Center => _center;

    public Color Color => _color;

    public float RotationDegrees => _rotationDegrees;

    public float Scale => _scale;

    public void MoveBy(Vector2 offset)
    {
        if (!IsFinite(offset))
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        _center += offset;
        OnMoved(offset);
    }

    public void ResizeBy(float factor)
    {
        if (!float.IsFinite(factor) || factor <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "Scale factor must be positive and finite.");
        }

        _scale *= factor;
        OnResized(factor);
    }

    public void ChangeColor(Color color)
    {
        _color = color;
        OnColorChanged(color);
    }

    public void RotateBy(float degrees)
    {
        if (!float.IsFinite(degrees))
        {
            throw new ArgumentOutOfRangeException(nameof(degrees));
        }

        _rotationDegrees = NormalizeDegrees(_rotationDegrees + degrees);
        OnRotated(degrees);
    }

    public abstract void Draw(IDrawContext context);

    public abstract bool Contains(Vector2 point);

    protected virtual void OnMoved(Vector2 offset)
    {
    }

    protected virtual void OnResized(float factor)
    {
    }

    protected virtual void OnColorChanged(Color color)
    {
    }

    protected virtual void OnRotated(float degrees)
    {
    }

    protected static Vector2 RotatePoint(Vector2 point, Vector2 origin, float degrees)
    {
        var radians = degrees * (MathF.PI / 180f);
        var offset = point - origin;
        var cosine = MathF.Cos(radians);
        var sine = MathF.Sin(radians);
        return origin + new Vector2(
            offset.X * cosine - offset.Y * sine,
            offset.X * sine + offset.Y * cosine);
    }

    protected static float NormalizeDegrees(float degrees)
    {
        var normalized = degrees % 360f;
        return normalized < 0f ? normalized + 360f : normalized;
    }

    private static bool IsFinite(Vector2 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y);
}
