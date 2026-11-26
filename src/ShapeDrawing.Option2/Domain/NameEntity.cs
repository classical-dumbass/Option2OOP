using System.Drawing;
using System.Numerics;

namespace ShapeDrawing.Option2.Domain;

public sealed class NameEntity : CompositeEntity
{
    public NameEntity(
        string text,
        Vector2 center,
        Color color,
        IEnumerable<DrawableEntity> glyphParts)
        : base(center, color)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Name text is required.", nameof(text));
        }

        ArgumentNullException.ThrowIfNull(glyphParts);
        var parts = glyphParts.ToArray();
        if (parts.Length == 0)
        {
            throw new ArgumentException("A name must contain at least one drawable part.", nameof(glyphParts));
        }

        Text = text;
        foreach (var part in parts)
        {
            AddChild(part);
        }
    }

    public string Text { get; }
}
