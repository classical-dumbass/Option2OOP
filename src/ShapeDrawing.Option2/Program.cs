using System.Numerics;
using ShapeDrawing.Option2.Application;
using ShapeDrawing.Option2.Domain;
using ShapeDrawing.Option2.Rendering;
using SplashKitSDK;
using ModelColor = System.Drawing.Color;

namespace ShapeDrawing.Option2;

internal static class Program
{
    private enum ShapeKind
    {
        Rectangle,
        Circle,
        Line
    }

    private static void Main()
    {
        Console.Write("Enter your first name (letters A-Z, spaces, and hyphens): ");
        var firstName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new InvalidOperationException("A first name is required to run the drawing app.");
        }
        firstName = NameGlyphFactory.Validate(firstName);

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
                scene.Draw(drawContext);
                DrawSelection(controller.SelectedName, drawContext);
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
            controller.MoveSelected(new Vector2(0f, -10f));
        }
        else if (SplashKit.KeyTyped(KeyCode.DownKey))
        {
            controller.MoveSelected(new Vector2(0f, 10f));
        }
        else if (SplashKit.KeyTyped(KeyCode.LeftKey))
        {
            controller.MoveSelected(new Vector2(-10f, 0f));
        }
        else if (SplashKit.KeyTyped(KeyCode.RightKey))
        {
            controller.MoveSelected(new Vector2(10f, 0f));
        }

        if (SplashKit.KeyTyped(KeyCode.PageUpKey))
        {
            controller.ResizeSelected(1.1f);
        }
        else if (SplashKit.KeyTyped(KeyCode.PageDownKey))
        {
            controller.ResizeSelected(0.9f);
        }

        if (SplashKit.KeyTyped(KeyCode.SpaceKey))
        {
            controller.RecolorSelected();
        }
    }

    private static DrawableEntity CreateShape(ShapeKind kind, Vector2 center, ModelColor color) =>
        kind switch
        {
            ShapeKind.Rectangle => new RectangleEntity(center, 80f, 60f, color),
            ShapeKind.Circle => new CircleEntity(center, 32f, color),
            ShapeKind.Line => new LineEntity(
                center + new Vector2(-35f, 25f),
                center + new Vector2(35f, -25f),
                color),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static Vector2 CurrentMousePosition() =>
        new(SplashKit.MouseX(), SplashKit.MouseY());

    private static ModelColor NextColor(Random random) =>
        ModelColor.FromArgb(
            random.Next(50, 256),
            random.Next(50, 256),
            random.Next(50, 256));

    private static void DrawSelection(NameEntity? selectedName, SplashKitDrawContext context)
    {
        if (selectedName is null)
        {
            return;
        }

        foreach (var part in selectedName.Children.OfType<LineEntity>())
        {
            context.DrawLine(
                part.Start,
                part.End,
                ModelColor.Gold,
                part.Thickness + 3f);
        }
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

        var selection = selectedName?.Text ?? selectedEntity?.GetType().Name ?? "none";
        SplashKit.DrawText(
            $"Mode: {shapeKind} | Name: {firstName} | Selected: {selection}",
            textColor,
            12d,
            50d);
    }
}
