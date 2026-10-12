# Validation · 0.4

12 October 2026: 74 original NUnit domain tests pass, including all 146 courses taught, weekly editing, specialist reservations, towers, migration, project deadlines and 360-day sustainable runs for DE/England/USA. Structure passes with 37 asset GUIDs. Core, runtime/UI and editor code compile against the official Unity 6000.0.65f1 assemblies and its bundled UGUI assembly.

Unity Android build was attempted with the official editor. Licensing initialization failed: LicenseClient IPC connection could not be established. The editor did not build an APK. No Unity gameplay/device test has passed yet. An offline Blender preview, if provided, renders the shared C# art recipes and is explicitly not a Unity screenshot.

Run `dotnet run --project tools/SchoolDomainTests.csproj --configuration Release` for domain tests. `tools/compile_unity_api.py` offers an additional API compilation check with an installed editor; it does not replace Unity tests.
