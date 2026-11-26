using System.Drawing;
using System.Numerics;

namespace ShapeDrawing.Option2.Domain;

public sealed class HouseEntity : CompositeEntity
{
    public HouseEntity(Vector2 center, Color color) : base(center, color)
    {
        AddChild(new RectangleEntity(center + new Vector2(0f, 12f), 72f, 52f, color));
        AddChild(new LineEntity(
            center + new Vector2(-42f, -14f),
            center + new Vector2(0f, -48f),
            color));
        AddChild(new LineEntity(
            center + new Vector2(0f, -48f),
            center + new Vector2(42f, -14f),
            color));
        AddChild(new CircleEntity(center + new Vector2(0f, 12f), 7f, color));
    }
}
