# Rektor · Prüfstand Godot 0.6.0 / Unity 0.5.0

## Godot und Android

13. Oktober 2026: Die Godot-4.5.1-.NET-Oberfläche kompiliert denselben Simulationskern wie Unity. 95 Simulationstests und 19 Prüfungen in der echten Godot-Szene bestehen. Geprüft sind Menü, Schwierigkeit, Schulleitung, Story, Personal, Bauen, 40 Schultage mit Ereignissen, sechs getrennte Spielstände, Sandbox, Raumkäufe und Kameraprojektion. Alle acht Verwaltungsseiten werden geöffnet.

Der native Android-Export wurde gebaut; APK-Struktur, Paketkennung und v2/v3-Signatur wurden geprüft. Der Veröffentlichungsworkflow installiert genau diese APK in einem beschleunigten Android-30-Emulator und verlangt dort dieselben 19 bestandenen Szenenprüfungen. Download, Prüfsumme und tatsächlich gerenderte Bilder: [Godot-Release](https://github.com/chekento/schooltycoon/releases/tag/rektor-godot-0.6.0). Physische ARM64-Telefone, Bildrate und Touch-Ergonomie benötigen weitere Spieltests.

## Unity

12. Oktober 2026: **95 NUnit-Simulationstests bestanden**, keine Fehler. Projektstruktur mit **43 eindeutigen Asset-GUIDs** geprüft. Core, Runtime einschließlich uGUI/Audio und Editor-Buildcode kompilieren gegen die offiziellen Unity-6000.0.65f1-Bibliotheken und die gebündelte uGUI-Assembly.

## Geprüfte Simulation

146 Fächer unterrichtet, unabhängige Kursnoten und Wochenpläne, individuelle Klassen, Fachraumreservierungen, Prüfungen, Versetzung, Abschlüsse, Forschung, AGs und Ereignisse. Sechs echte Geschosse und fünf vertikale Treppenübergänge. Preis-, Finanz- und Itemregeln, einmalige Käufe, dekorative Voraussetzungen, Upgrade-Grafikrezepte und die zwölf Story-Kapitel.

Sechs Spielstände mit echten serialisierten Dateien: Modi, Schwierigkeiten und Kursnoten bleiben getrennt. Beschädigte Hauptdateien nutzen eine Sicherung; eine ungültige neue Speicherung zerstört keinen bestehenden Spielstand. Migration von Version 2/3/4 bewahrt valide alte Daten.

## Langzeit-Balancing

720 Schultage pro Schwierigkeit mit eigenem Startkapital und tatsächlich verdientem Geld. Der automatische Spieler richtet Materialbudget, Versorgung, Personal, Forschung und Käufe nach den realen Bedingungen aus. Keine Erhöhung von Cash-Werten oder Freischalt-Abkürzung. Er erreichte je Schwierigkeit 228 Abschlüsse, zwölf Story-Kapitel, zwölf abgeschlossene jährliche Visionen und mindestens zwei Obergeschosse.

| Schwierigkeit | Niedrigster Kassenstand | Kassenstand nach 720 Tagen |
|---|---:|---:|
| Entspannt | 4.762 € | 851.432 € |
| Ausgewogen | 1.649 € | 473.493 € |
| Anspruchsvoll | 2.152 € | 243.262 € |

Zusätzlich laufen neue England- und USA-Kampagnen auf Anspruchsvoll 360 Tage nachhaltig. Das belegt spielbare Finanzwege, nicht die subjektive Qualität jeder möglichen Strategie oder langfristigen Unterhaltung.

## Grafik und Zielplattform

Die originalen C#-Modellrezepte wurden in Blender gerendert und visuell geprüft. Grundmöbel und gekaufte Items unterscheiden sich sichtbar. Die Hauptmenü-Illustration ist generiertes Artwork. Diese Bilder sind Modell-/Art-Prüfungen, keine Unity- oder Android-Aufnahmen.

Der echte Unity-Android-Build wurde lokal zweimal versucht. LicenseClient-IPC und Lizenzinitialisierung scheiterten. Der GitHub-Unity-APK-Workflow fand ebenfalls keine Aktivierungsdaten; Build und Artefakt-Upload wurden übersprungen. **Für Unity gibt es noch keine APK und keinen erfolgreichen Play-Mode-Lauf.** Der unabhängige Godot-Build benötigt diese Aktivierung nicht. [Unity-Build-Anleitung](ANDROID.md) · [Offene Prüfung](PLAYTEST.md).

## Reproduzieren

```bash
python3 tools/validate_project.py
dotnet run --project tools/SchoolDomainTests.csproj --configuration Release
```

Die Dateien unter `Assets/SchoolTycoon/Tests/EditMode` sind zugleich die Unity-Tests. `tools/compile_unity_api.py` prüft mit einem lokal installierten Editor die API-Kompilierung, führt aber Unity nicht aus. Auf Hosts, auf denen der dotnet-CLI-Prozess nicht geprüft werden kann, bietet `tools/compile_local.py` einen Roslyn-Fallback für dieselben Tests.
