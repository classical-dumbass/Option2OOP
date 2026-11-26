using System.Numerics;
using ShapeDrawing.Option2.Rendering;

namespace ShapeDrawing.Option2.Domain;

public sealed class Scene
{
    private readonly List<DrawableEntity> _entities = [];
    private readonly IReadOnlyList<DrawableEntity> _entitiesView;

    public Scene()
    {
        _entitiesView = _entities.AsReadOnly();
    }

    public IReadOnlyList<DrawableEntity> Entities => _entitiesView;

    public void Add(DrawableEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _entities.Add(entity);
    }

    public bool Remove(DrawableEntity entity) => _entities.Remove(entity);

    public DrawableEntity? FindTopmostAt(Vector2 point) =>
        _entities
            .AsEnumerable()
            .Reverse()
            .FirstOrDefault(entity => entity.Contains(point));

    public NameEntity? FindTopmostNameAt(Vector2 point) =>
        FindTopmostAt(point) as NameEntity;

    public void Draw(IDrawContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in _entities)
        {
            entity.Draw(context);
        }
    }

    public void RotateAll(float degrees)
    {
        foreach (var entity in _entities)
        {
            entity.RotateBy(degrees);
        }
    }
}
