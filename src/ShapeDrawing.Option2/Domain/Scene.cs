using System.Numerics;
using ShapeDrawing.Option2.Rendering;

namespace ShapeDrawing.Option2.Domain;

public sealed class Scene
{
    private readonly List<DrawableEntity> _entities = [];
    private readonly IReadOnlyList<DrawableEntity> _entitiesView;
    private readonly Stack<ISceneCommand> _undoCommands = new();
    private readonly Stack<ISceneCommand> _redoCommands = new();
    private List<DrawableEntity>? _batchEntities;
    private int _batchStartIndex;

    public Scene()
    {
        _entitiesView = _entities.AsReadOnly();
    }

    public IReadOnlyList<DrawableEntity> Entities
    {
        get
        {
            return _entitiesView;
        }
    }

    public bool CanUndo
    {
        get
        {
            return _undoCommands.Count > 0;
        }
    }

    public bool CanRedo
    {
        get
        {
            return _redoCommands.Count > 0;
        }
    }

    public void BeginBatch()
    {
        if (_batchEntities is not null)
        {
            throw new InvalidOperationException("A scene batch is already in progress.");
        }

        _batchStartIndex = _entities.Count;
        _batchEntities = new List<DrawableEntity>();
    }

    public void EndBatch()
    {
        if (_batchEntities is null)
        {
            throw new InvalidOperationException("No scene batch is in progress.");
        }

        if (_batchEntities.Count > 0)
        {
            PushUndoCommand(new BatchAddCommand(this, _batchEntities, _batchStartIndex));
        }

        _batchEntities = null;
    }

    public void Add(DrawableEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (_batchEntities is not null)
        {
            _entities.Add(entity);
            _batchEntities.Add(entity);
            return;
        }

        _entities.Add(entity);
        PushUndoCommand(new AddEntityCommand(this, entity, _entities.Count - 1));
    }

    public bool Remove(DrawableEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var index = _entities.IndexOf(entity);
        if (index < 0)
        {
            return false;
        }

        _entities.RemoveAt(index);
        PushUndoCommand(new RemoveEntityCommand(this, entity, index));
        return true;
    }

    public DrawableEntity? FindTopmostAt(Vector2 point)
    {
        for (var index = _entities.Count - 1; index >= 0; index--)
        {
            if (_entities[index].Contains(point))
            {
                return _entities[index];
            }
        }

        return null;
    }

    public NameEntity? FindTopmostNameAt(Vector2 point)
    {
        return FindTopmostAt(point) as NameEntity;
    }

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
        if (!float.IsFinite(degrees) || _entities.Count == 0)
        {
            return;
        }

        foreach (var entity in _entities)
        {
            entity.RotateBy(degrees);
        }

        PushUndoCommand(new RotateAllCommand(this, degrees));
    }

    public bool MoveEntity(DrawableEntity entity, Vector2 offset)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!_entities.Contains(entity) || !IsFinite(offset))
        {
            return false;
        }

        entity.MoveBy(offset);
        PushUndoCommand(new MoveEntityCommand(entity, offset));
        return true;
    }

    public bool ResizeEntity(DrawableEntity entity, float factor)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!_entities.Contains(entity) || !float.IsFinite(factor) || factor <= 0f)
        {
            return false;
        }

        entity.ResizeBy(factor);
        PushUndoCommand(new ResizeEntityCommand(entity, factor));
        return true;
    }

    public bool RecolorEntity(DrawableEntity entity, System.Drawing.Color color)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!_entities.Contains(entity))
        {
            return false;
        }

        var originalColor = entity.Color;
        entity.ChangeColor(color);
        PushUndoCommand(new RecolorEntityCommand(entity, originalColor, color));
        return true;
    }

    public bool Undo()
    {
        if (_undoCommands.Count == 0)
        {
            return false;
        }

        var command = _undoCommands.Pop();
        command.Undo();
        _redoCommands.Push(command);
        return true;
    }

    public bool Redo()
    {
        if (_redoCommands.Count == 0)
        {
            return false;
        }

        var command = _redoCommands.Pop();
        command.Redo();
        _undoCommands.Push(command);
        return true;
    }

    private void PushUndoCommand(ISceneCommand command)
    {
        _undoCommands.Push(command);
        _redoCommands.Clear();
    }

    private void AddWithoutHistory(DrawableEntity entity, int index)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var insertIndex = Math.Clamp(index, 0, _entities.Count);
        _entities.Insert(insertIndex, entity);
    }

    private bool RemoveWithoutHistory(DrawableEntity entity)
    {
        return _entities.Remove(entity);
    }

    private static bool IsFinite(Vector2 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y);
    }

    private interface ISceneCommand
    {
        void Undo();
        void Redo();
    }

    private sealed class AddEntityCommand : ISceneCommand
    {
        private readonly Scene _scene;
        private readonly DrawableEntity _entity;
        private readonly int _index;

        public AddEntityCommand(Scene scene, DrawableEntity entity, int index)
        {
            _scene = scene;
            _entity = entity;
            _index = index;
        }

        public void Undo()
        {
            _scene.RemoveWithoutHistory(_entity);
        }

        public void Redo()
        {
            _scene.AddWithoutHistory(_entity, _index);
        }
    }

    private sealed class RemoveEntityCommand : ISceneCommand
    {
        private readonly Scene _scene;
        private readonly DrawableEntity _entity;
        private readonly int _index;

        public RemoveEntityCommand(Scene scene, DrawableEntity entity, int index)
        {
            _scene = scene;
            _entity = entity;
            _index = index;
        }

        public void Undo()
        {
            _scene.AddWithoutHistory(_entity, _index);
        }

        public void Redo()
        {
            _scene.RemoveWithoutHistory(_entity);
        }
    }

    private sealed class BatchAddCommand : ISceneCommand
    {
        private readonly Scene _scene;
        private readonly IReadOnlyList<DrawableEntity> _entities;
        private readonly int _startIndex;

        public BatchAddCommand(Scene scene, IReadOnlyList<DrawableEntity> entities, int startIndex)
        {
            _scene = scene;
            _entities = entities;
            _startIndex = startIndex;
        }

        public void Undo()
        {
            for (var index = _entities.Count - 1; index >= 0; index--)
            {
                _scene.RemoveWithoutHistory(_entities[index]);
            }
        }

        public void Redo()
        {
            for (var index = 0; index < _entities.Count; index++)
            {
                _scene.AddWithoutHistory(_entities[index], _startIndex + index);
            }
        }
    }

    private sealed class MoveEntityCommand : ISceneCommand
    {
        private readonly DrawableEntity _entity;
        private readonly Vector2 _offset;

        public MoveEntityCommand(DrawableEntity entity, Vector2 offset)
        {
            _entity = entity;
            _offset = offset;
        }

        public void Undo()
        {
            _entity.MoveBy(-_offset);
        }

        public void Redo()
        {
            _entity.MoveBy(_offset);
        }
    }

    private sealed class ResizeEntityCommand : ISceneCommand
    {
        private readonly DrawableEntity _entity;
        private readonly float _factor;

        public ResizeEntityCommand(DrawableEntity entity, float factor)
        {
            _entity = entity;
            _factor = factor;
        }

        public void Undo()
        {
            _entity.ResizeBy(1f / _factor);
        }

        public void Redo()
        {
            _entity.ResizeBy(_factor);
        }
    }

    private sealed class RecolorEntityCommand : ISceneCommand
    {
        private readonly DrawableEntity _entity;
        private readonly System.Drawing.Color _originalColor;
        private readonly System.Drawing.Color _newColor;

        public RecolorEntityCommand(
            DrawableEntity entity,
            System.Drawing.Color originalColor,
            System.Drawing.Color newColor)
        {
            _entity = entity;
            _originalColor = originalColor;
            _newColor = newColor;
        }

        public void Undo()
        {
            _entity.ChangeColor(_originalColor);
        }

        public void Redo()
        {
            _entity.ChangeColor(_newColor);
        }
    }

    private sealed class RotateAllCommand : ISceneCommand
    {
        private readonly Scene _scene;
        private readonly float _degrees;

        public RotateAllCommand(Scene scene, float degrees)
        {
            _scene = scene;
            _degrees = degrees;
        }

        public void Undo()
        {
            foreach (var entity in _scene._entities)
            {
                entity.RotateBy(-_degrees);
            }
        }

        public void Redo()
        {
            foreach (var entity in _scene._entities)
            {
                entity.RotateBy(_degrees);
            }
        }
    }
}
