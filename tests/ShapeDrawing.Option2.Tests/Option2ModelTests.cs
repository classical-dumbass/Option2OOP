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

        Assert.IsTrue(scene.Entities.Count >= 3 && scene.Entities.Count <= 10);
        Assert.IsTrue(scene.Entities.All(entity => entity is HouseEntity));
        foreach (var house in scene.Entities.Cast<HouseEntity>())
        {
            Assert.AreEqual(1, house.Children.OfType<RectangleEntity>().Count());
            Assert.AreEqual(1, house.Children.OfType<CircleEntity>().Count());
            Assert.AreEqual(2, house.Children.OfType<LineEntity>().Count());
        }
    }

    [TestMethod]
    public void CompositeTransforms_AffectItsPartsAsOneObject()
    {
        var house = new HouseEntity(new Vector2(100f, 100f), Color.Blue);
        var originalCenter = house.Center;
        var originalChildCenters = house.Children.Select(child => child.Center).ToArray();
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

        var centersBeforeRotation = house.Children.Select(child => child.Center).ToArray();
        house.RotateBy(90f);

        Assert.AreEqual(90f, house.RotationDegrees);
        for (var index = 0; index < house.Children.Count; index++)
        {
            var expected = RotatePoint(centersBeforeRotation[index], expectedCenter, 90f);
            AssertVectorApproximately(expected, house.Children[index].Center);
            Assert.AreEqual(90f, house.Children[index].RotationDegrees);
        }

        house.ChangeColor(Color.Green);
        Assert.IsTrue(house.Children.All(child => child.Color == Color.Green));
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

        Assert.IsTrue(scene.Entities.All(entity => entity.RotationDegrees == expectedAngle));
        var house = (HouseEntity)scene.Entities[2];
        Assert.IsTrue(house.Children.All(child => child.RotationDegrees == expectedAngle));
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
        Assert.IsTrue(name.Children.All(part => part is LineEntity && part.Color == Color.Purple));
        Assert.Throws<ArgumentException>(() =>
            NameGlyphFactory.Create("A1", Vector2.Zero, Color.Black));
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
