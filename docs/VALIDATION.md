# Prüfstand — 0.2.0

Datum: **2026-10-08**.

## Durchgeführt

| Prüfung | Ergebnis |
| --- | --- |
| Pure C#-Simulation mit Mono C# Compiler kompiliert | Bestanden |
| Dieselben NUnit-Testmethoden aus dem Unity-Testordner separat ausgeführt | **22 bestanden, 0 fehlgeschlagen** |
| Kampagnentest: alle sieben Förderziele bis mindestens 96 Schüler erreichbar | Bestanden |
| 60-Tage-Test: Kapazität, Solvenz und valide Speicherzustände | Bestanden |
| C#-Syntax aller zehn Quelldateien mit Tree-sitter geprüft | Bestanden |
| Unity-Dateistruktur, Asset-Metadaten und eindeutige GUIDs | Bestanden |
| Startszene und Build-Szenenreferenz | Bestanden |
| Input-Manager-Achsen und aktiver klassischer Input | Bestanden |
| Mitgelieferter Shader im Resources-Ordner | Bestanden |
| GitHub-Upload: alle 58 Projektdateien auf `main` in `chekento/schooltycoon` | Bestanden |
| GitHub Actions: Projektstruktur, .NET 8 / NUnit 3.13.3 | **22 bestanden, 0 fehlgeschlagen** |
| uGUI-2.0.0-Assemblyname mit Unitys offizieller 6000.0-Quelle abgeglichen | `UnityEngine.UI` bestätigt |

Die lokalen Tests verwendeten NUnit 2.6.4 und Mono 6.8. Zusätzlich lief derselbe Testsatz erfolgreich in GitHub Actions mit .NET 8 und NUnit 3.13.3: [erster erfolgreicher Prüflauf](https://github.com/chekento/schooltycoon/actions/runs/37841114407). Die Unity-Testumgebung verwendet das Unity Test Framework 1.6.0; deren Ausführung im Editor ist noch ausstehend.

## Noch ausstehend

- Import und vollständige C#-Kompilierung im Unity-Editor.
- Shader-Kompilierung, tatsächliches Rendering und visuelle Prüfung im Spiel.
- Menübedienung, Bildschirmgrößen, Touchgesten, Kamerasteuerung und Figurenwege in Play Mode.
- Speichern und Wiederherstellen mit Unitys `JsonUtility` auf den jeweiligen Plattformen.
- Windows-, Linux-, WebGL- und Android-Builds.
- APK-Installation und Spieltest auf einem Android-Gerät.

**Keine fertige Anwendung / APK ist Teil dieses Projektpakets.** Die beiliegende Campusillustration ist eine Illustration des Stils und Raumkonzepts, kein Screenshot oder Beleg eines Unity-Renders.

## Tests selbst wiederholen

Unity: **Window → General → Test Runner → EditMode → Run All**.

Ohne Unity-Editor, mit .NET 8:

```sh
python3 tools/validate_project.py
dotnet run --project tools/SchoolDomainTests.csproj --configuration Release
```

Die Domain-Tests prüfen unter anderem: verbundene Räume, gedrehte Bauflächen, fehlendes Geld, einmalige Baukosten, einmalige Fördergelder, Personalbedarf, Nichtüberbelegung, Tagesabrechnung, Rückbau, Wegsuche, Ereignissperren, kostenlose Ereignisentscheidung bei Schulden, Speicherstandsvalidierung und Zahlungsunfähigkeit.

## Technische Quellen

- [Unity 6.0: uGUI](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.ugui.html)
- [Unity 6.0: Test Framework](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.test-framework.html)
- [Offizielle uGUI-2.0.0-Paketdefinition](https://github.com/Unity-Technologies/uGUI/blob/6000.0/com.unity.ugui/package.json)
- [Offizielle uGUI-Assemblydefinition](https://github.com/Unity-Technologies/uGUI/blob/6000.0/com.unity.ugui/Runtime/UGUI/UnityEngine.UI.asmdef)
- [Unity 6.0: NamedBuildTarget](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Build.NamedBuildTarget.FromBuildTargetGroup.html)
