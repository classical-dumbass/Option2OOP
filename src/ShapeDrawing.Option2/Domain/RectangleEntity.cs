using System.Drawing;
using System.Numerics;
using ShapeDrawing.Option2.Rendering;

namespace ShapeDrawing.Option2.Domain;

public sealed class RectangleEntity : DrawableEntity
{
    private float _width;
    private float _height;

    public RectangleEntity(
        Vector2 center,
        float width,
        float height,
        Color color,
        float rotationDegrees = 0f)
        : base(center, color)
    {
        ValidateDimension(width, nameof(width));
        ValidateDimension(height, nameof(height));

        _width = width;
        _height = height;
        RotateBy(rotationDegrees);
    }

    public float Width
    {
        get
        {
            return _width;
        }
        set
        {
            ValidateDimension(value, nameof(value));
            _width = value;
        }
    }

    public float Height
    {
        get
        {
            return _height;
        }
        set
        {
            ValidateDimension(value, nameof(value));
            _height = value;
        }
    }

    public override void Draw(IDrawContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.DrawRectangle(Center, _width, _height, Color, RotationDegrees);
    }

    public override bool Contains(Vector2 point)
    {
        var localPoint = RotatePoint(point, Center, -RotationDegrees);
        return MathF.Abs(localPoint.X - Center.X) <= _width / 2f
            && MathF.Abs(localPoint.Y - Center.Y) <= _height / 2f;
    }

    protected override void OnResized(float factor)
    {
        _width *= factor;
        _height *= factor;
    }

    private static void ValidateDimension(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0f)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Dimension must be positive and finite.");
        }
    }
}
