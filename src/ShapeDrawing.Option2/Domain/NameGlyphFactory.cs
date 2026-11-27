using System.Drawing;
using System.Numerics;

namespace ShapeDrawing.Option2.Domain;

public static class NameGlyphFactory
{
    private const float CellSize = 8f;
    private static readonly IReadOnlyDictionary<char, string[]> Patterns =
        new Dictionary<char, string[]>
        {
            ['A'] = ["01110", "10001", "10001", "11111", "10001", "10001", "10001"],
            ['B'] = ["11110", "10001", "10001", "11110", "10001", "10001", "11110"],
            ['C'] = ["01111", "10000", "10000", "10000", "10000", "10000", "01111"],
            ['D'] = ["11110", "10001", "10001", "10001", "10001", "10001", "11110"],
            ['E'] = ["11111", "10000", "10000", "11110", "10000", "10000", "11111"],
            ['F'] = ["11111", "10000", "10000", "11110", "10000", "10000", "10000"],
            ['G'] = ["01111", "10000", "10000", "10111", "10001", "10001", "01111"],
            ['H'] = ["10001", "10001", "10001", "11111", "10001", "10001", "10001"],
            ['I'] = ["11111", "00100", "00100", "00100", "00100", "00100", "11111"],
            ['J'] = ["00111", "00010", "00010", "00010", "10010", "10010", "01100"],
            ['K'] = ["10001", "10010", "10100", "11000", "10100", "10010", "10001"],
            ['L'] = ["10000", "10000", "10000", "10000", "10000", "10000", "11111"],
            ['M'] = ["10001", "11011", "10101", "10101", "10001", "10001", "10001"],
            ['N'] = ["10001", "11001", "10101", "10011", "10001", "10001", "10001"],
            ['O'] = ["01110", "10001", "10001", "10001", "10001", "10001", "01110"],
            ['P'] = ["11110", "10001", "10001", "11110", "10000", "10000", "10000"],
            ['Q'] = ["01110", "10001", "10001", "10001", "10101", "10010", "01101"],
            ['R'] = ["11110", "10001", "10001", "11110", "10100", "10010", "10001"],
            ['S'] = ["01111", "10000", "10000", "01110", "00001", "00001", "11110"],
            ['T'] = ["11111", "00100", "00100", "00100", "00100", "00100", "00100"],
            ['U'] = ["10001", "10001", "10001", "10001", "10001", "10001", "01110"],
            ['V'] = ["10001", "10001", "10001", "10001", "10001", "01010", "00100"],
            ['W'] = ["10001", "10001", "10001", "10101", "10101", "10101", "01010"],
            ['X'] = ["10001", "10001", "01010", "00100", "01010", "10001", "10001"],
            ['Y'] = ["10001", "10001", "01010", "00100", "00100", "00100", "00100"],
            ['Z'] = ["11111", "00001", "00010", "00100", "01000", "10000", "11111"],
            ['-'] = ["00000", "00000", "00000", "11111", "00000", "00000", "00000"],
            [' '] = ["00000", "00000", "00000", "00000", "00000", "00000", "00000"]
        };

    public static NameEntity Create(string text, Vector2 center, Color color)
    {
        var normalizedText = Validate(text);
        var advance = CellSize * 6f;
        var totalWidth = (normalizedText.Length * advance) - CellSize;
        var left = center.X - (totalWidth / 2f);
        var top = center.Y - ((7 * CellSize) / 2f);
        var parts = new List<DrawableEntity>();

        for (var letterIndex = 0; letterIndex < normalizedText.Length; letterIndex++)
        {
            var pattern = Patterns[normalizedText[letterIndex]];
            for (var row = 0; row < 7; row++)
            {
                var column = 0;
                while (column < 5)
                {
                    if (pattern[row][column] != '1')
                    {
                        column++;
                        continue;
                    }

                    var runStart = column;
                    while (column + 1 < 5 && pattern[row][column + 1] == '1')
                    {
                        column++;
                    }

                    var y = top + (row * CellSize) + (CellSize / 2f);
                    var x = left + (letterIndex * advance);
                    parts.Add(new LineEntity(
                        new Vector2(x + (runStart * CellSize), y),
                        new Vector2(x + ((column + 1) * CellSize), y),
                        color,
                        CellSize - 1f));
                    column++;
                }
            }
        }

        return new NameEntity(normalizedText, center, color, parts);
    }

    public static string Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Name text is required.", nameof(text));
        }

        var normalizedText = text.Trim().ToUpperInvariant();
        var containsUnsupportedCharacter = false;
        var containsOnlySpaces = true;
        foreach (var character in normalizedText)
        {
            if (!Patterns.ContainsKey(character))
            {
                containsUnsupportedCharacter = true;
            }

            if (character != ' ')
            {
                containsOnlySpaces = false;
            }
        }

        if (containsUnsupportedCharacter || containsOnlySpaces)
        {
            throw new ArgumentException(
                "Name must contain letters A-Z; spaces and hyphens are also supported.",
                nameof(text));
        }

        return normalizedText;
    }
}
