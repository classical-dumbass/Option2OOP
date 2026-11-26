# Option 2 implementation planner

## Current status

The project now has a C# model, a SplashKit application, a basic name-glyph
factory, and five passing model tests. `make build` and `make test` both work.
The app recreates the documented Week 4 workflow because the actual Week 4
source project was not supplied. It has not yet been manually checked against
the program used in class.

The app asks for the first name when it starts. The student has not chosen or
provided the name yet, so the name can be entered at runtime for now.

## Done

- [x] Initialize Git and add `.gitignore`, `README.md`, `doc/`, `src/`, and
  `tests/`.
- [x] Add an abstract `DrawableEntity` model, concrete shapes, composites,
  a single top-level scene collection, and a feature controller in one
  student-facing .NET project.
- [x] Add a name glyph factory for A-Z, spaces, and hyphens.
- [x] Implement the model action for creating 3 to 10 randomly colored houses.
- [x] Implement name-composite selection, movement, resizing, and recoloring.
- [x] Implement one randomly selected rotation angle applied to every
  top-level scene object.
- [x] Add the SplashKit renderer and a runnable app with an event loop.
- [x] Recreate the tutorial's R/C/L shape selection and click-to-add controls.
- [x] Add H for houses, N for the name, and T for rotating all objects.
- [x] Add mouse selection, Delete, arrow-key movement, Page Up/Down resizing,
  and Space recoloring for the name.
- [x] Add draft class and name-selection sequence diagrams.
- [x] Add the editable report template at `doc/report_draft.docx`.
- [x] Add `make build`, `make run`, `make test`, `make clean`, and `make help`.
- [x] Configure application output to go into the repository-root `build/`.
- [x] Build the app with no warnings and run five passing model tests.

## 1. Confirm the project and personalization

- [x] Review the Week 4 PDF and note the documented R/C/L shape keys and
  mouse-based drawing workflow.
- [x] Confirm that the app should be recreated from the tutorial because the
  original project source is not available.
- [ ] Choose the first name to use in the final demonstration.
- [ ] If the original Week 4 project becomes available, compare its controls
  and behavior with this recreation.
- [ ] Confirm the app's mouse controls and new key bindings are acceptable.

## 2. Manually verify the app

- [ ] Run `make run` in an environment with SplashKit's native library
  installed.
- [ ] Try R/C/L and left-click placement; right-click selection; and Delete.
- [ ] Try H and confirm the random house count, shape parts, positions, and
  colors look right on the canvas.
- [ ] Enter the chosen first name, press N, and check that the letters are
  readable and fit the canvas.
- [ ] Right-click the name and test arrow-key movement, Page Up/Down resizing,
  and Space recoloring.
- [ ] Press T with primitive and composite objects present; check that each
  rotates correctly and composites stay together.
- [ ] Check that Escape closes the app and that the controls remain usable
  after repeated actions.

## 3. Finish the report and submission

- [ ] Update the UML drafts to match the final code, then produce complete,
  readable diagrams using the CASE tool accepted by the course.
- [ ] Complete `doc/report_draft.docx`: enter student details, add the final
  diagrams, and record only verified test results.
- [ ] Record the demonstration video, showing the features and explaining the
  OOP design. Display the student's name and ID at the beginning.
- [ ] Complete the required lab instructor check-in and submit source code,
  video link, and UML report through Canvas.

The assignment lists the Week 12 deadline as 27 November 2026 at 23:59; confirm
the Canvas page for any updates.
