# Source layout

`ShapeDrawing.Option2` is the single C# console/SplashKit project. Its
`Application/` and `Domain/` folders contain the model and feature logic;
`Program.cs` and `Rendering/` connect them to the window and input events. The
implementation recreates the Week 4 tutorial because the original C# project
was not included in the workspace.

The project is arranged so that:

- `Rendering/IDrawContext.cs` defines the model's drawing boundary, implemented
  by `Rendering/SplashKitDrawContext.cs`.
- `Program.cs` forwards SplashKit events to `Application/SceneController.cs`.
- Keep all scene-level drawables in `Domain/Scene.cs`.
- Do not add a composite's child shapes to the scene independently.

`make build` places the application output in the repository-root `build/`
folder.

Install the SplashKit native library using the course setup instructions before
running the application.
