# 🏫 The School Simulation 0.2

**Grow one classroom into a thriving campus.** An original Unity school tycoon with an isometric 3D view, cheerful procedural cartoon visuals, and German / English UI.

[Deutsch](../README.md) · [Quick start](QUICKSTART.de.md) · [Validation](VALIDATION.md)

![Illustrative campus view; not a Unity screenshot](campus-preview.svg)

## Current version

**0.2.0 — source prototype.** The project contains the game simulation, runtime scene builder, UI, original procedural visuals, tests and build commands. A Unity editor was unavailable during development. The pure C# simulation has been compiled and tested, but the Unity editor, rendering, UI, target builds and Android installation have **not** been verified. No APK is bundled.

## Launch in Unity

1. Add this folder in Unity Hub.
2. Open with Unity **6000.0.65f1**, allow packages to import.
3. Open `Assets/Scenes/Campus.unity` and press **Play**.
4. Choose your director's character, hair colour and name.
5. Hire a teacher in the **Staff** tab, build from the left-hand palette, then press **Play** in the game controls.

You begin paused with a corridor, one classroom, 18 students and €16,000. A teacher is needed to staff the first 24 seats.

## Implemented gameplay

- Ten room types: corridor, classroom, restroom, staff room, canteen, library, garden, science lab, art studio and gym.
- Connected-corridor construction rules, rotated footprints, coloured placement previews and demolition with 50% refund.
- Daily applicants: teachers, caretakers and counselors.
- Six lesson blocks; maths, languages, science, arts and sports. Specialist staff and rooms affect learning.
- Education grants, canteen income, wages, maintenance and configurable supplies / admissions.
- Learning, happiness, cleanliness and reputation; daily enrollment and withdrawals.
- Seven progression grants and recurring school events with three choices each.
- 20×16 owned tiles, expandable to the full 32×24 campus.
- Corridor-based student and staff movement, lunchtime and garden time.
- A persistent status bar, management menus, pause / speed controls, German / English switch.
- Drag-to-pan, mouse wheel / pinch zoom, touch controls and screen safe area.
- Local JSON saves with validation and a previous-save backup.

Camera: drag / WASD, wheel, Q / E. Rotate room: R. Pause: Space. Cancel building: Esc. Touch controls use the on-screen buttons.

## Builds and tests

Use **School Simulation → Build** for Windows, Linux, Android APK or WebGL. Install target support in Unity Hub first. Android uses ARM64, IL2CPP, Android API 26+ and landscape orientation.

Domain tests are under `Assets/SchoolTycoon/Tests/EditMode`. Run them in the Unity Test Runner or use `.NET 8`:

```sh
python3 tools/validate_project.py
dotnet run --project tools/SchoolDomainTests.csproj --configuration Release
```

The standard GitHub workflow runs these checks. A separate manual Unity build workflow requires your own licensed self-hosted Unity runner. The game uses the Built-in Render Pipeline, uGUI and the legacy Input Manager. No Asset Store purchase is required.

## Prototype limits

The current world uses procedural shapes. Student totals are authoritative in the domain; the view caps visual students at 64 and visual staff at 24. People use corridor routes but do not yet avoid one another. Timetables and educational outcomes are school-wide aggregates. Saves target desktop and Android local storage; WebGL save persistence after closing the browser still requires target-specific verification.

Concept by KoSch / The School Simulation. No distribution licence selected yet. Unity packages retain their respective terms.
