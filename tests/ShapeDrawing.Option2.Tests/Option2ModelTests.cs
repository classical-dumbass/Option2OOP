using System.Drawing;
using System.Numerics;
using ShapeDrawing.Option2.Application;
using ShapeDrawing.Option2.Domain;

namespace ShapeDrawing.Option2.Tests;

[TestClass]
public sealed class Option2ModelTests
{
    [TestMethod]
    public void AddRandomHouses_CreatesBetweenThreeAndTenHouseComposites()
    {
        var scene = new Scene();
        var controller = new SceneController(scene, new Random(42));

        controller.AddRandomHouses(640f, 480f);

        Assert.IsInRange(3, 10, scene.Entities.Count);
        Assert.IsNotEmpty(scene.Entities);
        foreach (var entity in scene.Entities)
        {
            Assert.IsInstanceOfType<HouseEntity>(entity);
        }
        foreach (var house in scene.Entities.Cast<HouseEntity>())
        {
            Assert.AreEqual(1, house.Children.OfType<RectangleEntity>().Count());
            Assert.AreEqual(1, house.Children.OfType<CircleEntity>().Count());
            Assert.AreEqual(2, house.Children.OfType<LineEntity>().Count());
            Assert.IsInRange(50f, 590f, house.Center.X);
            Assert.IsInRange(55f, 425f, house.Center.Y);
        }

        Assert.AreNotEqual(scene.Entities[0].Color, scene.Entities[1].Color);
    }

    [TestMethod]
    public void CompositeTransforms_AffectItsPartsAsOneObject()
    {
        var house = new HouseEntity(new Vector2(100f, 100f), Color.Blue);
        var originalCenter = house.Center;
        var originalChildCenters = new Vector2[house.Children.Count];
        for (var index = 0; index < house.Children.Count; index++)
        {
            originalChildCenters[index] = house.Children[index].Center;
        }
        var offset = new Vector2(5f, -3f);

        house.MoveBy(offset);
        house.ResizeBy(2f);

        var expectedCenter = originalCenter + offset;
        Assert.AreEqual(expectedCenter, house.Center);
        for (var index = 0; index < house.Children.Count; index++)
        {
            var expected = expectedCenter + ((originalChildCenters[index] - originalCenter) * 2f);
            Assert.AreEqual(expected, house.Children[index].Center);
        }

        var centersBeforeRotation = new Vector2[house.Children.Count];
        for (var index = 0; index < house.Children.Count; index++)
        {
            centersBeforeRotation[index] = house.Children[index].Center;
        }
        house.RotateBy(90f);

        Assert.AreEqual(90f, house.RotationDegrees);
        for (var index = 0; index < house.Children.Count; index++)
        {
            var expected = RotatePoint(centersBeforeRotation[index], expectedCenter, 90f);
            AssertVectorApproximately(expected, house.Children[index].Center);
            Assert.AreEqual(90f, house.Children[index].RotationDegrees);
        }

        house.ChangeColor(Color.Green);
        foreach (var child in house.Children)
        {
            Assert.AreEqual(Color.Green, child.Color);
        }
    }

    [TestMethod]
    public void RotateAll_AppliesOneAngleToEveryTopLevelDrawableAndCompositePart()
    {
        var scene = new Scene();
        scene.Add(new RectangleEntity(new Vector2(20f, 20f), 30f, 20f, Color.Red));
        scene.Add(new CircleEntity(new Vector2(50f, 50f), 8f, Color.Blue));
        scene.Add(new HouseEntity(new Vector2(100f, 100f), Color.Green));
        var controller = new SceneController(scene, new Random(7));

        var angle = controller.RotateAll();
        var expectedAngle = ((angle % 360f) + 360f) % 360f;

        foreach (var entity in scene.Entities)
        {
            Assert.AreEqual(expectedAngle, entity.RotationDegrees);
        }
        var house = (HouseEntity)scene.Entities[2];
        foreach (var child in house.Children)
        {
            Assert.AreEqual(expectedAngle, child.RotationDegrees);
        }
    }

    [TestMethod]
    public void SelectedName_CanBeMovedResizedAndRecolored()
    {
        var scene = new Scene();
        var originalColor = Color.Black;
        var name = new NameEntity(
            "A",
            new Vector2(100f, 100f),
            originalColor,
            [new RectangleEntity(new Vector2(100f, 100f), 20f, 30f, originalColor)]);
        var controller = new SceneController(scene, new Random(1));
        controller.AddName(name);

        Assert.IsTrue(controller.SelectNameAt(new Vector2(100f, 100f)));
        Assert.IsTrue(controller.MoveSelected(new Vector2(10f, 5f)));
        Assert.AreEqual(new Vector2(110f, 105f), name.Center);
        Assert.IsTrue(controller.ResizeSelected(2f));
        Assert.AreEqual(40f, name.Children.OfType<RectangleEntity>().Single().Width);
        Assert.IsTrue(controller.RecolorSelected());
        Assert.AreEqual(name.Color, name.Children.Single().Color);
        Assert.AreNotEqual(originalColor, name.Color);

        Assert.IsFalse(controller.SelectNameAt(new Vector2(500f, 500f)));
        Assert.IsFalse(controller.MoveSelected(Vector2.One));
    }

    [TestMethod]
    public void NameGlyphFactory_CreatesDrawableLettersAndRejectsUnsupportedCharacters()
    {
        var name = NameGlyphFactory.Create(
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ -",
            new Vector2(200f, 150f),
            Color.Purple);

        Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXYZ -", name.Text);
        Assert.IsGreaterThan(100, name.Children.Count);
        foreach (var part in name.Children)
        {
            Assert.IsInstanceOfType<LineEntity>(part);
            Assert.AreEqual(Color.Purple, part.Color);
        }

        Assert.Throws<ArgumentException>(delegate
        {
            NameGlyphFactory.Create("A1", Vector2.Zero, Color.Black);
        });
    }

    [TestMethod]
    public void SceneUndoAndRedo_RestoreAddedAndRemovedEntities()
    {
        var scene = new Scene();
        var entity = new CircleEntity(new Vector2(50f, 50f), 10f, Color.Red);

        scene.Add(entity);
        Assert.HasCount(1, scene.Entities);
        Assert.IsTrue(scene.Undo());
        Assert.IsEmpty(scene.Entities);
        Assert.IsTrue(scene.Redo());
        Assert.HasCount(1, scene.Entities);

        Assert.IsTrue(scene.Remove(entity));
        Assert.IsEmpty(scene.Entities);
        Assert.IsTrue(scene.Undo());
        Assert.HasCount(1, scene.Entities);
        Assert.IsTrue(scene.Redo());
        Assert.IsEmpty(scene.Entities);
    }

    [TestMethod]
    public void SceneUndoAndRedo_RestoreEntityTransforms()
    {
        var scene = new Scene();
        var entity = new RectangleEntity(
            new Vector2(100f, 100f),
            40f,
            30f,
            Color.Red);
        scene.Add(entity);

        scene.MoveEntity(entity, new Vector2(10f, 5f));
        Assert.AreEqual(new Vector2(110f, 105f), entity.Center);
        Assert.IsTrue(scene.Undo());
        Assert.AreEqual(new Vector2(100f, 100f), entity.Center);
        Assert.IsTrue(scene.Redo());
        Assert.AreEqual(new Vector2(110f, 105f), entity.Center);

        scene.ResizeEntity(entity, 2f);
        Assert.AreEqual(80f, entity.Width);
        Assert.IsTrue(scene.Undo());
        Assert.AreEqual(40f, entity.Width);
        Assert.IsTrue(scene.Redo());
        Assert.AreEqual(80f, entity.Width);

        scene.RecolorEntity(entity, Color.Blue);
        Assert.AreEqual(Color.Blue, entity.Color);
        Assert.IsTrue(scene.Undo());
        Assert.AreEqual(Color.Red, entity.Color);
        Assert.IsTrue(scene.Redo());
        Assert.AreEqual(Color.Blue, entity.Color);
    }

    private static Vector2 RotatePoint(Vector2 point, Vector2 origin, float degrees)
    {
        var radians = degrees * (MathF.PI / 180f);
        var offset = point - origin;
        return origin + new Vector2(
            (offset.X * MathF.Cos(radians)) - (offset.Y * MathF.Sin(radians)),
            (offset.X * MathF.Sin(radians)) + (offset.Y * MathF.Cos(radians)));
    }

    private static void AssertVectorApproximately(Vector2 expected, Vector2 actual)
    {
        Assert.AreEqual(expected.X, actual.X, 0.001f);
        Assert.AreEqual(expected.Y, actual.Y, 0.001f);
    }
}
