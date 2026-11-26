using System.Drawing;
using System.Numerics;

namespace ShapeDrawing.Option2.Rendering;

public interface IDrawContext
{
    void DrawLine(Vector2 start, Vector2 end, Color color, float thickness);

    void DrawRectangle(
        Vector2 center,
        float width,
        float height,
        Color color,
        float rotationDegrees);

    void DrawCircle(Vector2 center, float radius, Color color);
}
