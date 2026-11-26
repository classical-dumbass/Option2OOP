using System.Drawing;
using System.Numerics;
using ShapeDrawing.Option2.Rendering;

namespace ShapeDrawing.Option2.Domain;

public sealed class LineEntity : DrawableEntity
{
    private Vector2 _start;
    private Vector2 _end;
    private float _thickness;

    public LineEntity(Vector2 start, Vector2 end, Color color, float thickness = 2f)
        : base((start + end) / 2f, color)
    {
        if (!float.IsFinite(thickness) || thickness <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(thickness));
        }

        _start = start;
        _end = end;
        _thickness = thickness;
    }

    public Vector2 Start => _start;

    public Vector2 End => _end;

    public float Thickness => _thickness;

    public override void Draw(IDrawContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.DrawLine(_start, _end, Color, _thickness);
    }

    public override bool Contains(Vector2 point)
    {
        var segment = _end - _start;
        var lengthSquared = segment.LengthSquared();
        if (lengthSquared <= float.Epsilon)
        {
            return Vector2.DistanceSquared(point, _start) <= _thickness * _thickness;
        }

        var projection = Math.Clamp(Vector2.Dot(point - _start, segment) / lengthSquared, 0f, 1f);
        var closestPoint = _start + projection * segment;
        var tolerance = MathF.Max(_thickness, 6f);
        return Vector2.DistanceSquared(point, closestPoint) <= tolerance * tolerance;
    }

    protected override void OnMoved(Vector2 offset)
    {
        _start += offset;
        _end += offset;
    }

    protected override void OnResized(float factor)
    {
        _start = Center + ((_start - Center) * factor);
        _end = Center + ((_end - Center) * factor);
        _thickness *= factor;
    }

    protected override void OnRotated(float degrees)
    {
        _start = RotatePoint(_start, Center, degrees);
        _end = RotatePoint(_end, Center, degrees);
    }
}
