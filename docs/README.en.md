# 🏫 The School Simulation 0.3

Grow a classroom into a thriving campus. This Unity school tycoon uses original procedural cartoon visuals and German / English menus.

[Repository](https://github.com/chekento/schooltycoon) · [Deutsch](../README.md) · [Quick start](QUICKSTART.de.md) · [Gameplay rules](GAMEPLAY.md) · [Validation](VALIDATION.md)

![Illustrative campus view; not a Unity screenshot](campus-preview.svg)

## Open the game

Add the project in Unity Hub, use **6000.0.65f1**, allow packages to import, then open `Assets/Scenes/Campus.unity` and press Play. The empty edit-time scene is intentional: the application constructs its campus, camera and menus at runtime.

Choose your director's character, hair and name. You start paused with one corridor, one classroom, 18 pupil profiles and €16,000. Hire a teacher in **Staff**, build facilities from the left palette and start time or press **End day**.

## Gameplay in 0.3.0

- Persistent pupils with five subject scores, favourite subjects, grades, wellbeing, stress and attendance. Three individual support choices with a daily limit.
- Assigned classes with names, individual timetables, pupil transfers and unique teacher assignment. Funding follows the pupils actually served by their own class.
- Sixty-day school years, exams every twenty days, report cards, promotion, repeating, sixth-grade graduation and fresh admissions. Sandbox play continues after the seven progression grants.
- Teachers, caretakers and counselors with energy, morale, meaningful training and recurring salary increases.
- Ten room types with connected-corridor placement, rotation, previews, demolition, wear, repairs and two upgrade levels. Classrooms grow from 24 to 32 seats.
- Four research projects with facility prerequisites, one-time costs, daily progress and permanent effects.
- Five clubs with their own required rooms, retained equipment and recurring operating costs.
- Twelve events with three choices each, six achievement grants, daily reports and detailed budget policies.
- Routes based on actual class timetables, specialist facilities, lunch and clubs. Absentees are hidden. Simplified weather and seasonal garden colours.
- Eight management tabs, DE/EN switching, touch gestures, camera drag/zoom/orbit, pause and speed controls.
- Validated v3 JSON saves and backups; automatic migration of valid v2 saves preserves campus, money, staff, director and progress.

Each system is connected to the simulation and has an in-game menu. Clubs simplify participation: every present pupil receives the active programmes' learning bonuses, while the view distributes people between their rooms. The renderer caps visible pupils at 64 and staff at 24; every profile is simulated independently of that cap. People use corridor routes without mutual collision avoidance. There is no audio.

## Verification and builds

This is **0.3.0 source code**, with separately compiled and tested C# gameplay. A Unity editor was unavailable. Unity import, rendering, actual menu interaction, platform builds and device installation remain unverified. No APK is bundled. See [validation](VALIDATION.md) and the [Play Mode checklist](PLAYTEST.md).

Run the same domain tests in Unity Test Runner or with .NET 8:

```sh
python3 tools/validate_project.py
dotnet run --project tools/SchoolDomainTests.csproj --configuration Release
```

GitHub Actions runs these checks. A separate manual build workflow requires a licensed self-hosted Unity runner. **School Simulation → Build** provides Windows, Linux, Android APK and WebGL commands; install the appropriate Unity Hub target module first. Android output is `Builds/Android/TheSchoolSimulation-0.3.0.apk`. WebGL save persistence across browser sessions still needs target-specific verification.

Built-in Render Pipeline, uGUI 2.0.0, legacy Input Manager. No Asset Store purchase required. Concept: KoSch / The School Simulation; independent Unity implementation of the selected school simulation plugin. No distribution licence selected. Unity packages retain their own terms.
