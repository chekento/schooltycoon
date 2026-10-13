# Rektor · Godot 4 migration

Godot is implemented as a second, independent frontend in `Godot/`. The original Unity project at the repository root is retained, including its scene, runtime, shaders, menus and Android build method. No Unity activation is needed by Godot.

Both frontends compile **the same C# domain files** from `Assets/SchoolTycoon/Core/`. There is no reduced or separately rebalanced simulation: difficulty rules, 146 built-in subjects, electives, pupil profiles, weekly plans, research, clubs, story, room items, six-floor construction, events and save validation remain shared. The Godot 3D renderer uses the same original `CampusDesign` recipes for rooms, furniture, landscape and characters.

## Start Godot

Use **Godot 4.5.1 .NET**, .NET SDK 8 and 9, and open `Godot/project.godot`. The Android target uses .NET 9; desktop uses .NET 8. Godot's C# Android support is officially experimental.

```bash
python3 tools/setup_godot.py --dotnet --templates
dotnet build Godot/Rektor.csproj
godot --path Godot --editor --import
godot --path Godot
```

`setup_godot.py` prints the exact installed Godot executable and .NET SDK directory. Add the SDK directory to `PATH` and use the printed executable in place of `godot`. Installation and export require no account login.

## Android

The GitHub workflow `Rektor Godot Android` installs official tools, compiles the frontend, runs native scene smoke checks, exports the APK, checks its signature, then installs and launches that APK in an accelerated Android 30 emulator. It does not depend on the Unity secrets. Successful main-branch builds publish the [0.6.0 preview release](https://github.com/chekento/schooltycoon/releases/tag/rektor-godot-0.6.0), including the APK, SHA-256 checksum and actual screenshots. Branch builds are available as Actions artifacts.

For a local Linux build, install OpenJDK 17, then:

```bash
python3 tools/setup_android.py
# Set JAVA_HOME to OpenJDK 17 and ANDROID_HOME to the SDK directory printed above.
godot --headless --path Godot --editor --script res://Build/ConfigureAndroid.gd
godot --headless --path Godot --export-debug Android ../Builds/Android/Rektor-0.6.0-godot.apk
```

The preset uses ARM64 and x86-64, landscape, offline gameplay and a native OpenGL compatibility renderer. Its separate package ID is **`cloud.kosch.rektor.godot`**; the Unity version retains **`cloud.kosch.rektor`**, so both can be installed side by side.

`Godot/Build/debug.keystore` is an **intentionally public development key**, not a secret or a production signing identity. It keeps preview APK updates install-compatible. A Play Store release needs an owner's private release keystore and an AAB. Do not use this development key for production.

## Unity remains available

Open the root project in Unity 6000.0.65f1. Use **Rektor → Build → Android APK** once Unity is activated. The original Android workflow is retained and can be launched manually. Automatic Unity builds run only when Unity assets/settings change, not for Godot-only commits.

Both engines use save schema 5 and the same checksum/backup archive format. Godot can import Unity `.save` archives and legacy JSON via the main menu. Each engine has its own app-data directory and six independent slots; saves are not silently overwritten across engines. Copying a valid archive into Unity's matching slot path enables exchange on desktop; a future Unity-native Android import UI can be added separately.

## Verification status

The native Godot scene passed **19 automated frontend checks** locally: boot, main menu, difficulty, the three-step director designer, Mara guidance, hiring, connected room construction, 40 school days and event choices, six isolated save slots, backup serialization, sandbox construction, room upgrades, continuing a campaign and camera picking. All eight management pages instantiated. The shared simulation already has its independent 95-test suite, including 720-day balance runs.

The published APK passed export, archive integrity, application identity and Android v2/v3 signature verification. All **19 native scene checks also passed inside the installed APK** on an accelerated Android 30 x86-64 emulator with 4 GB RAM, the ANGLE/SwiftShader renderer and a 4 GB partition. The [successful build and release run](https://github.com/chekento/schooltycoon/actions/runs/38461310461) records both desktop and Android results. The shared 95-test suite passed for the same commit.

APK SHA-256: `1c39ba2b0edb8e9ee674556c48f5088f8c0a22712d1c357eeb0391921045c555`. Size: 207,899,202 bytes. Preview screenshots come from the running Godot frontend; the main-menu background is original generated artwork. An emulator check does not establish frame rate, touch ergonomics or compatibility on every physical ARM64 phone. Godot C# Android remains experimental; this is a development preview, not a Play Store submission.

Official documentation: [Android export](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_android.html), [C# requirements](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html), [Godot license](https://godotengine.org/license/).
