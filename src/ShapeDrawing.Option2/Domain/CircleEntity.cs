using System.Drawing;
using System.Numerics;
using ShapeDrawing.Option2.Rendering;

namespace ShapeDrawing.Option2.Domain;

public sealed class CircleEntity : DrawableEntity
{
    private float _radius;

    public CircleEntity(Vector2 center, float radius, Color color) : base(center, color)
    {
        ValidateRadius(radius);
        _radius = radius;
    }

    public float Radius
    {
        get
        {
            return _radius;
        }
        set
        {
            ValidateRadius(value);
            _radius = value;
        }
    }

    public override void Draw(IDrawContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.DrawCircle(Center, _radius, Color);
    }

    public override bool Contains(Vector2 point)
    {
        return Vector2.DistanceSquared(Center, point) <= _radius * _radius;
    }

    protected override void OnResized(float factor)
    {
        _radius *= factor;
    }

    private static void ValidateRadius(float value)
    {
        if (!float.IsFinite(value) || value <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Radius must be positive and finite.");
        }
    }
}
