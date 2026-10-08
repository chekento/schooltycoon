# Prüfstand — 0.3.0

Datum: **2026-10-08**. Dieses Dokument unterscheidet ausführbare Domain-Prüfungen von noch ausstehenden Unity-Prüfungen.

## Durchgeführt

| Prüfung | Ergebnis |
| --- | --- |
| Pure C#-Simulation mit Mono 6.8 kompiliert | Bestanden |
| Dieselben NUnit-Testmethoden aus dem Unity-Testordner separat ausgeführt | **55 bestanden, 0 fehlgeschlagen** |
| Ursprüngliche Kampagne bis zu allen sieben Förderzielen und mindestens 96 Schülern | Bestanden |
| Gesamtkampagne mit allen vier Forschungen, fünf AGs und freiem Weiterspielen | Bestanden |
| Langzeitablauf über 660 Schultage: Versetzung, Abschlüsse, Neuaufnahme und begrenzte Historien | Bestanden |
| Eigenständige Klassenpläne, Lehrerwechsel, Schülerwechsel und tatsächliche Finanzierung | Bestanden |
| Individuelle Förderung, Lernfortschritt, Abwesenheit und Prüfungen | Bestanden |
| Einmalige Forschungs-, Ausstattungs-, Fortbildungs- und Ausbaukosten | Bestanden |
| Wiederaufnahme pausierter AGs ohne neue Ausstattungsgebühr | Bestanden |
| Migration gültiger v2-Stände und Ablehnung beschädigter / unbekannter Formate | Bestanden |
| C#-Syntax aller 14 Quelldateien mit Tree-sitter geprüft | Bestanden |
| Unity-Dateistruktur, Metadaten und eindeutige GUIDs | Bestanden: 27 GUIDs |
| Startszene, Build-Szenenreferenz, Input-Manager-Achsen und Runtime-Shader | Bestanden |
| GitHub Actions mit .NET 8 und NUnit 3.13.3 | **55 bestanden, 0 fehlgeschlagen** |

Lokale Domain-Prüfungen verwendeten NUnit 2.6.4. GitHub Actions führt denselben Testsatz mit .NET 8 und NUnit 3.13.3 aus: [Simulation checks](https://github.com/chekento/schooltycoon/actions). Für Version 0.3.0 wurden dort ebenfalls **55 Tests erfolgreich** ausgeführt: [erfolgreicher Prüflauf](https://github.com/chekento/schooltycoon/actions/runs/37845868592). Unity selbst verwendet das Unity Test Framework 1.6.0; dessen Ausführung im Editor ist noch ausstehend.

## Noch ausstehend

- Import und vollständige C#-Kompilierung im Unity-Editor, einschließlich Runtime- und UI-Assemblies.
- Shader-Kompilierung, Rendering, saisonale Farben und Sichtbarkeit der Ausbaustufen.
- Tatsächliche Bedienung aller acht Menüs, Modalfenster, Namen, Profile und Berichte.
- Bildschirmgrößen, Touchgesten, Kamerasteuerung und Figurenwege in Play Mode.
- JSON-Speicherung und v2-Migration mit Unitys `JsonUtility` auf den Zielplattformen.
- Windows-, Linux-, WebGL- und Android-Builds; APK-Installation und Spieltest am Gerät.
- WebGL-Persistenz nach dem Schließen und erneuten Öffnen des Browsers.

Die konkrete Bedienungs- und Plattformprüfliste steht in [PLAYTEST.md](PLAYTEST.md). **Keine geprüfte fertige Anwendung oder APK liegt bei.** Die Campusillustration ist kein Unity-Screenshot.

## Prüfungen wiederholen

Unity: **Window → General → Test Runner → EditMode → Run All**.

Ohne Unity-Editor, mit .NET 8:

```sh
python3 tools/validate_project.py
dotnet run --project tools/SchoolDomainTests.csproj --configuration Release
```

Die Testdateien `SchoolSimulationTests.cs` und `AcademyTests.cs` bilden eine gemeinsame Fixture. Beide werden im Unity-Testordner und vom separat laufenden Testprojekt verwendet. Der separate Runner führt ausschließlich Domain-Tests aus, keine Unity-Menüs oder Grafik.

## Technische Quellen

- [Unity 6.0: uGUI](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.ugui.html)
- [Unity 6.0: Test Framework](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.test-framework.html)
- [Offizielle uGUI-2.0.0-Paketdefinition](https://github.com/Unity-Technologies/uGUI/blob/6000.0/com.unity.ugui/package.json)
- [Offizielle uGUI-Assemblydefinition](https://github.com/Unity-Technologies/uGUI/blob/6000.0/com.unity.ugui/Runtime/UGUI/UnityEngine.UI.asmdef)
- [Unity 6.0: NamedBuildTarget](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Build.NamedBuildTarget.FromBuildTargetGroup.html)
