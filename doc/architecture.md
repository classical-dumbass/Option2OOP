# Architecture and OOP design

## Design goals

- Keep the Week 4 tutorial's documented shape-drawing workflow while adding
  the Option 2 interactions.
- Keep scene/state rules in ordinary C# model classes, isolated from the
  graphics library.
- Put every top-level drawable in one `Scene` collection typed as
  `DrawableEntity`; draw and transform through that common base type.
- Model houses and names as composites whose child shapes transform together.
- Keep mutable state private and expose read-only properties plus behavior
  methods.

## Class responsibilities

| Type | Responsibility |
| --- | --- |
| `DrawableEntity` | Abstract base for position, color, scale, and rotation; defines common draw and hit-test operations and transform hooks. |
| `LineEntity` | Encapsulates line endpoints and draws/tests/transforms a line. |
| `RectangleEntity` | Encapsulates dimensions and rotation-aware drawing/hit testing. |
| `CircleEntity` | Encapsulates radius; participates in transforms even though rotation has no visible effect. |
| `CompositeEntity` | Owns child `DrawableEntity` objects and propagates draw, move, resize, color, and rotation operations. |
| `HouseEntity` | One composite made from rectangle, circle, and line entities. |
| `NameEntity` | One composite containing the line strokes that form the configured first name. |
| `NameGlyphFactory` | Validates a first name and creates its letter shapes for a `NameEntity`. |
| `Scene` | Owns the single top-level `List<DrawableEntity>`, finds the topmost drawable/name under a point, and provides add/remove/draw/scene-wide operation entry points. |
| `SceneController` | Implements feature actions: create 3–10 houses, select a name, edit the selected name, and rotate all scene objects by one random angle. |
| `IDrawContext` | Rendering boundary that separates model geometry from a graphics library. |
| `SplashKitDrawContext` | Implements the rendering boundary with SplashKit drawing calls, including rotated rectangle outlines. |
| `Program` | Runs the SplashKit event loop, maps keyboard/mouse input to model actions, and prompts for the first name. |

`SceneController` receives the application's random source (or a seeded source
in tests), so feature behavior can be checked without putting random-number
generation into individual shapes. The app translates SplashKit key/mouse
events into controller calls.

## Relationships and OOP principles

- **Abstraction:** `DrawableEntity` defines what any drawable can do without
  exposing the details of a line, rectangle, circle, or composite.
- **Encapsulation:** coordinates, dimensions, child lists, and scene storage are
  private. Callers change state through validated methods such as `MoveBy`,
  `ResizeBy`, `ChangeColor`, and `RotateBy`.
- **Inheritance:** primitive entities and composites derive from
  `DrawableEntity`; house and name derive from `CompositeEntity`.
- **Polymorphism:** `Scene` stores base-class references and invokes the common
  API. Concrete types provide their own drawing, hit-testing, and transform
  behavior.
- **Composition:** a house and a name own their component shapes. The scene
  treats each composite as one top-level object, so selection and rotation do
  not split a composite apart.
- **Single responsibility:** input translation, scene management, shape
  geometry, and SplashKit rendering are kept in separate classes.

The nested parts of a composite are owned by that composite; top-level scene
operations act on the single scene collection. Do not separately add a
composite's parts to the scene, or those parts may be drawn/transformed twice.

## Rotation contract

`SceneController` chooses one angle for a rotation command and asks `Scene` to
apply that same angle to every top-level entity. Primitive line endpoints and
rectangle orientation must rotate about their own centers. A circle accepts
rotation but has no visible orientation change. A composite rotates child
positions around its own center and rotates each child by the same angle.
Coordinate conventions and SplashKit's rotation direction should be confirmed
during integration.

## Current controls

The Week 4 tutorial documents `R`, `C`, and `L` for choosing the shape type and
mouse clicks for adding shapes. This recreation uses left-click to add and
right-click to select; `Delete` removes the selected object. Option 2 uses
`H` for houses, `N` for the entered name, and `T` for scene-wide rotation.
When a name is selected, arrow keys move it, Page Up/Down resize it, and Space
recolors it. These controls should be compared with the class version if its
source becomes available.

## Project boundary

The model classes do not depend on SplashKit. The `Program` entry point and
rendering adapter connect them to the window and input loop, but all of these
classes are built as one project to keep the student setup simple. Since the
original Week 4 source project was not provided, the app recreates the behavior
described in its tutorial; confirm the input details match the version used in
class.

The focused tests are in `tests/ShapeDrawing.Option2.Tests/`. They cover the
house count and component types, composite transformations, same-angle
scene rotation, and name selection/edit actions. They do not replace testing
inside the integrated SplashKit application.

The Mermaid files in `doc/diagrams/` are working drafts. For the report, redraw
or import the final, verified class diagram and at least one sequence diagram
in the CASE tool expected by the course, and include complete readable
illustrations.
