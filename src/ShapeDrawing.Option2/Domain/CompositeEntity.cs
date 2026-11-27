using System.Drawing;
using System.Numerics;
using ShapeDrawing.Option2.Rendering;

namespace ShapeDrawing.Option2.Domain;

public abstract class CompositeEntity : DrawableEntity
{
    private readonly List<DrawableEntity> _children = [];
    private readonly IReadOnlyList<DrawableEntity> _childrenView;

    protected CompositeEntity(Vector2 center, Color color) : base(center, color)
    {
        _childrenView = _children.AsReadOnly();
    }

    public IReadOnlyList<DrawableEntity> Children
    {
        get
        {
            return _childrenView;
        }
    }

    protected void AddChild(DrawableEntity child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
    }

    public override void Draw(IDrawContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var child in _children)
        {
            child.Draw(context);
        }
    }

    public override bool Contains(Vector2 point)
    {
        foreach (var child in _children)
        {
            if (child.Contains(point))
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnMoved(Vector2 offset)
    {
        foreach (var child in _children)
        {
            child.MoveBy(offset);
        }
    }

    protected override void OnResized(float factor)
    {
        foreach (var child in _children)
        {
            var relativePosition = child.Center - Center;
            child.MoveBy(relativePosition * (factor - 1f));
            child.ResizeBy(factor);
        }
    }

    protected override void OnColorChanged(Color color)
    {
        foreach (var child in _children)
        {
            child.ChangeColor(color);
        }
    }

    protected override void OnRotated(float degrees)
    {
        foreach (var child in _children)
        {
            var rotatedCenter = RotatePoint(child.Center, Center, degrees);
            child.MoveBy(rotatedCenter - child.Center);
            child.RotateBy(degrees);
        }
    }
}
