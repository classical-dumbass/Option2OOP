using System.Numerics;
using ShapeDrawing.Option2.Application;
using ShapeDrawing.Option2.Domain;
using ShapeDrawing.Option2.Rendering;
using SplashKitSDK;
using ModelColor = System.Drawing.Color;

namespace ShapeDrawing.Option2;

internal static class Program
{
    private const float RectangleWidth = 80f;
    private const float RectangleHeight = 60f;
    private const float CircleRadius = 32f;
    private const float LineOffset = 35f;
    private const float LineCrossOffset = 25f;
    private const float MoveDistance = 10f;
    private const float ResizeUpFactor = 1.1f;
    private const float ResizeDownFactor = 0.9f;

    private enum ShapeKind
    {
        Rectangle,
        Circle,
        Line
    }

    private static void Main()
    {
        string firstName;
        while (true)
        {
            Console.Write("Enter your first name (letters A-Z, spaces, and hyphens): ");
            var input = Console.ReadLine();

            try
            {
                firstName = NameGlyphFactory.Validate(input ?? string.Empty);
                break;
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        var random = new Random();
        var scene = new Scene();
        var controller = new SceneController(scene, random);
        var drawContext = new SplashKitDrawContext();
        var shapeKind = ShapeKind.Rectangle;
        DrawableEntity? selectedEntity = null;
        var window = SplashKit.OpenWindow("ShapeDrawing - Option 2", 900, 650);

        try
        {
            while (!window.CloseRequested && !SplashKit.QuitRequested())
            {
                SplashKit.ProcessEvents();
                if (SplashKit.KeyTyped(KeyCode.EscapeKey))
                {
                    break;
                }

                HandleInput(
                    scene,
                    controller,
                    window,
                    random,
                    firstName,
                    ref shapeKind,
                    ref selectedEntity);

                SplashKit.ClearScreen(SplashKit.RGBColor(245, 245, 245));
                DrawSelection(controller.SelectedName, drawContext);
                scene.Draw(drawContext);
                DrawInstructions(shapeKind, selectedEntity, controller.SelectedName, firstName);
                SplashKit.RefreshScreen();
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static void HandleInput(
        Scene scene,
        SceneController controller,
        Window window,
        Random random,
        string firstName,
        ref ShapeKind shapeKind,
        ref DrawableEntity? selectedEntity)
    {
        var controlDown = SplashKit.KeyDown(KeyCode.LeftCtrlKey)
            || SplashKit.KeyDown(KeyCode.RightCtrlKey);

        if (controlDown && SplashKit.KeyTyped(KeyCode.ZKey))
        {
            scene.Undo();
            selectedEntity = null;
            controller.ClearNameSelection();
        }

        if (controlDown
            && (SplashKit.KeyTyped(KeyCode.YKey)
                || (SplashKit.KeyDown(KeyCode.LeftShiftKey)
                    && SplashKit.KeyTyped(KeyCode.ZKey))))
        {
            scene.Redo();
            selectedEntity = null;
            controller.ClearNameSelection();
        }

        if (SplashKit.KeyTyped(KeyCode.RKey))
        {
            shapeKind = ShapeKind.Rectangle;
        }
        else if (SplashKit.KeyTyped(KeyCode.CKey))
        {
            shapeKind = ShapeKind.Circle;
        }
        else if (SplashKit.KeyTyped(KeyCode.LKey))
        {
            shapeKind = ShapeKind.Line;
        }

        if (SplashKit.KeyTyped(KeyCode.HKey))
        {
            controller.AddRandomHouses(window.Width, window.Height);
        }

        if (SplashKit.KeyTyped(KeyCode.NKey))
        {
            var center = new Vector2(window.Width / 2f, window.Height / 2f);
            controller.AddName(NameGlyphFactory.Create(firstName, center, NextColor(random)));
        }

        if (SplashKit.KeyTyped(KeyCode.TKey))
        {
            controller.RotateAll();
        }

        if (SplashKit.MouseClicked(MouseButton.LeftButton))
        {
            var point = CurrentMousePosition();
            selectedEntity = null;
            controller.ClearNameSelection();
            scene.Add(CreateShape(shapeKind, point, NextColor(random)));
        }

        if (SplashKit.MouseClicked(MouseButton.RightButton))
        {
            var point = CurrentMousePosition();
            selectedEntity = scene.FindTopmostAt(point);
            controller.SelectNameAt(point);
        }

        if (SplashKit.KeyTyped(KeyCode.DeleteKey) && selectedEntity is not null)
        {
            scene.Remove(selectedEntity);
            selectedEntity = null;
            controller.ClearNameSelection();
        }

        if (controller.SelectedName is null)
        {
            return;
        }

        if (SplashKit.KeyTyped(KeyCode.UpKey))
        {
            controller.MoveSelected(new Vector2(0f, -MoveDistance));
        }
        else if (SplashKit.KeyTyped(KeyCode.DownKey))
        {
            controller.MoveSelected(new Vector2(0f, MoveDistance));
        }
        else if (SplashKit.KeyTyped(KeyCode.LeftKey))
        {
            controller.MoveSelected(new Vector2(-MoveDistance, 0f));
        }
        else if (SplashKit.KeyTyped(KeyCode.RightKey))
        {
            controller.MoveSelected(new Vector2(MoveDistance, 0f));
        }

        if (SplashKit.KeyTyped(KeyCode.PageUpKey))
        {
            controller.ResizeSelected(ResizeUpFactor);
        }
        else if (SplashKit.KeyTyped(KeyCode.PageDownKey))
        {
            controller.ResizeSelected(ResizeDownFactor);
        }

        if (SplashKit.KeyTyped(KeyCode.SpaceKey))
        {
            controller.RecolorSelected();
        }
    }

    private static DrawableEntity CreateShape(ShapeKind kind, Vector2 center, ModelColor color)
    {
        switch (kind)
        {
            case ShapeKind.Rectangle:
                return new RectangleEntity(center, RectangleWidth, RectangleHeight, color);
            case ShapeKind.Circle:
                return new CircleEntity(center, CircleRadius, color);
            case ShapeKind.Line:
                return new LineEntity(
                    center + new Vector2(-LineOffset, LineCrossOffset),
                    center + new Vector2(LineOffset, -LineCrossOffset),
                    color);
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private static Vector2 CurrentMousePosition()
    {
        return new Vector2(SplashKit.MouseX(), SplashKit.MouseY());
    }

    private static ModelColor NextColor(Random random)
    {
        return ModelColor.FromArgb(
            random.Next(50, 256),
            random.Next(50, 256),
            random.Next(50, 256));
    }

    private static void DrawSelection(NameEntity? selectedName, SplashKitDrawContext context)
    {
        if (selectedName is null)
        {
            return;
        }

        var lineParts = selectedName.Children.OfType<LineEntity>().ToArray();
        if (lineParts.Length == 0)
        {
            return;
        }

        var left = float.PositiveInfinity;
        var right = float.NegativeInfinity;
        var top = float.PositiveInfinity;
        var bottom = float.NegativeInfinity;

        foreach (var part in lineParts)
        {
            var partLeft = MathF.Min(part.Start.X, part.End.X);
            var partRight = MathF.Max(part.Start.X, part.End.X);
            var partTop = MathF.Min(part.Start.Y, part.End.Y);
            var partBottom = MathF.Max(part.Start.Y, part.End.Y);

            left = MathF.Min(left, partLeft);
            right = MathF.Max(right, partRight);
            top = MathF.Min(top, partTop);
            bottom = MathF.Max(bottom, partBottom);
        }

        left -= 6f;
        right += 6f;
        top -= 6f;
        bottom += 6f;
        var topLeft = new Vector2(left, top);
        var topRight = new Vector2(right, top);
        var bottomRight = new Vector2(right, bottom);
        var bottomLeft = new Vector2(left, bottom);

        context.DrawLine(topLeft, topRight, ModelColor.Gold, 3f);
        context.DrawLine(topRight, bottomRight, ModelColor.Gold, 3f);
        context.DrawLine(bottomRight, bottomLeft, ModelColor.Gold, 3f);
        context.DrawLine(bottomLeft, topLeft, ModelColor.Gold, 3f);
    }

    private static void DrawInstructions(
        ShapeKind shapeKind,
        DrawableEntity? selectedEntity,
        NameEntity? selectedName,
        string firstName)
    {
        var textColor = SplashKit.RGBColor(20, 20, 20);
        SplashKit.DrawText(
            "R/C/L: choose shape | Left click: add | Right click: select | Delete: remove",
            textColor,
            12d,
            10d);
        SplashKit.DrawText(
            "H: houses | N: draw name | T: rotate all | Arrows: move name | PgUp/PgDn: resize | Space: recolor",
            textColor,
            12d,
            30d);
        SplashKit.DrawText(
            "Ctrl+Z: undo | Ctrl+Y or Ctrl+Shift+Z: redo",
            textColor,
            12d,
            70d);

        var selection = selectedName?.Text ?? selectedEntity?.GetType().Name ?? "none";
        SplashKit.DrawText(
            $"Mode: {shapeKind} | Name: {firstName} | Selected: {selection}",
            textColor,
            12d,
            50d);
    }
}
