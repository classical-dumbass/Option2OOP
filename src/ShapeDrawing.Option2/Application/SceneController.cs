using System.Drawing;
using System.Numerics;
using ShapeDrawing.Option2.Domain;

namespace ShapeDrawing.Option2.Application;

public sealed class SceneController
{
    private readonly Scene _scene;
    private readonly Random _random;
    private NameEntity? _selectedName;

    public SceneController(Scene scene, Random random)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(random);
        _scene = scene;
        _random = random;
    }

    public NameEntity? SelectedName => _selectedName;

    public void AddRandomHouses(float canvasWidth, float canvasHeight)
    {
        const float horizontalMargin = 50f;
        const float verticalMargin = 55f;
        if (!float.IsFinite(canvasWidth) || canvasWidth <= horizontalMargin * 2f)
        {
            throw new ArgumentOutOfRangeException(nameof(canvasWidth));
        }

        if (!float.IsFinite(canvasHeight) || canvasHeight <= verticalMargin * 2f)
        {
            throw new ArgumentOutOfRangeException(nameof(canvasHeight));
        }

        var count = _random.Next(3, 11);
        for (var index = 0; index < count; index++)
        {
            var center = new Vector2(
                NextFloat(horizontalMargin, canvasWidth - horizontalMargin),
                NextFloat(verticalMargin, canvasHeight - verticalMargin));
            _scene.Add(new HouseEntity(center, NextColor()));
        }
    }

    public void AddName(NameEntity name)
    {
        ArgumentNullException.ThrowIfNull(name);
        _scene.Add(name);
    }

    public void ClearNameSelection()
    {
        _selectedName = null;
    }

    public bool SelectNameAt(Vector2 point)
    {
        _selectedName = _scene.FindTopmostNameAt(point);
        return _selectedName is not null;
    }

    public bool MoveSelected(Vector2 offset)
    {
        if (_selectedName is null)
        {
            return false;
        }

        _selectedName.MoveBy(offset);
        return true;
    }

    public bool ResizeSelected(float factor)
    {
        if (_selectedName is null)
        {
            return false;
        }

        _selectedName.ResizeBy(factor);
        return true;
    }

    public bool RecolorSelected()
    {
        if (_selectedName is null)
        {
            return false;
        }

        _selectedName.ChangeColor(NextColor());
        return true;
    }

    public float RotateAll()
    {
        var degrees = _random.Next(-180, 181);
        _scene.RotateAll(degrees);
        return degrees;
    }

    private float NextFloat(float minimum, float maximum) =>
        minimum + ((float)_random.NextDouble() * (maximum - minimum));

    private Color NextColor() =>
        Color.FromArgb(_random.Next(50, 256), _random.Next(50, 256), _random.Next(50, 256));
}
