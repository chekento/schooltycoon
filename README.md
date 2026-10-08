# 🏫 The School Simulation 0.3

**Baue eine kleine Schule zu einem lebendigen Campus aus.** Ein eigenständiger Schul-Tycoon in Unity mit isometrischer 3D-Ansicht, freundlicher Cartoon-Grafik und einer deutschen und englischen Oberfläche.

[GitHub](https://github.com/chekento/schooltycoon) · [Prüfungen](https://github.com/chekento/schooltycoon/actions) · [English](docs/README.en.md) · [Schnellstart](docs/QUICKSTART.de.md) · [Spielsysteme](docs/GAMEPLAY.md) · [Änderungen](CHANGELOG.md)

![Illustrative Campusansicht, kein Unity-Screenshot](docs/campus-preview.svg)

## Aktueller Stand

Version **0.3.0 – Unity-Prototyp**. Das Repository enthält das vollständige Quellprojekt, eine Startszene, eigene prozedurale Raum- und Figurengrafik, Spiellogik, Tests und Build-Menüs. Es enthält noch **keine im Unity-Editor geprüfte Anwendung und keine fertige APK**. Der Unity-Editor war in der Entwicklungsumgebung nicht installiert; die C#-Simulation wurde separat kompiliert und getestet. Der genaue Prüfstand steht in [VALIDATION.md](docs/VALIDATION.md).

## Spielen

1. Projekt in **Unity Hub → Add project from disk** hinzufügen.
2. Mit **Unity 6.0, 6000.0.65f1** öffnen und Paketimport abwarten.
3. `Assets/Scenes/Campus.unity` öffnen, **Play** drücken.
4. Charakter, Haarfarbe und Namen wählen.
5. Rechts im Tab **Team** eine Lehrkraft einstellen. Links Bauwerkzeuge auswählen.
6. Auf **Start** klicken oder mit **Tag beenden** einen Schultag abrechnen.

Der Campus beginnt mit einem Flur, einem Klassenraum, **18 Schülern und 16.000 €**. Du startest in der Planungspause. Eine Lehrkraft wird gebraucht, bevor die 24 Unterrichtsplätze betreut sind.

## Gameplay in Version 0.3.0

- **Individuelle Schüler:** dauerhafte Profile, fünf Fachwerte, Lieblingsfach, Jahrgang, Wohlbefinden, Stress, Anwesenheit und Prüfungsstand. Förderunterricht, Beratung oder Gespräch: eine Maßnahme je Schüler und Tag.
- **Feste Klassen:** eigene Namen und Stundenpläne, konkrete Lehrerzuordnung, Schülerwechsel und überprüfte Platzgrenzen. Personalwechsel wirken auf die tatsächlich betroffene Klasse und ihre Bildungsförderung.
- **Schuljahre:** 60 Schultage mit drei Prüfungen und Zeugnissen. Versetzung, Wiederholung, Abschluss nach Jahrgang 6 und anschließende Neuaufnahme; laufender Schulbetrieb auch nach allen Förderzielen.
- **Personalentwicklung:** tägliche Bewerbungen, drei Berufsrollen, Energie, Teamgefühl und drei Fortbildungsstufen mit einmaligen Gebühren und höherem Tagesgehalt.
- **Campuspflege:** zehn Bautypen, Bauvorschau, Drehung, Fluranschluss, Rückbau, Raumverschleiß, Reparaturen und zwei Ausbaustufen. Klassenräume erhalten bis zu 32 Plätze; andere Räume verbessern Versorgung, Erholung, Forschung oder Unterricht.
- **Forschung:** vier Projekte mit Voraussetzungen, einmaligen Startkosten und täglichem Fortschritt. Nachhaltigkeit senkt Betriebskosten; digitale, individuelle und kreative Lernansätze verändern Schülerentwicklung.
- **Fünf AGs:** Garten, Lesen, Robotik, Theater und Sport mit benötigten Räumen, einmaliger Ausstattung und laufenden Kosten. Pausierte AGs behalten ihre Ausstattung.
- **Entscheidungen:** zwölf Schulsituationen mit je drei Wegen und sichtbaren Auswirkungen. Sieben Förderziele und sechs zusätzliche Erfolge mit einmaligen Sonderförderungen.
- **Finanzen:** Einnahmen, Gehälter, Raumkosten, Lernmaterial, AGs, Förderung und Essen; Tagesprognose, Abrechnung und Berichte. Alle laufenden Kosten werden einmal je Schultag berechnet.
- **Lebendiger Campus:** Figuren folgen Klassenzuordnung und Stundenplan, besuchen Fachräume, Mensa und AGs. Wetter verändert freie Nachmittagsziele; vier Jahreszeiten verändern Gartenfarben.
- **Bedienung:** acht Verwaltungsansichten, Charakterdesigner, deutsche und englische Oberfläche, frei bewegliche Kamera, Zoom, Pause, Zeitbeschleunigung und Touchgesten.
- **Speichern:** validierte lokale Spielstände mit Sicherung und automatischer Migration von Version 0.2.0.

Die Regeln und ihre Kosten stehen in [GAMEPLAY.md](docs/GAMEPLAY.md). Die ersten Schritte und neue Menüs erklärt [QUICKSTART.de.md](docs/QUICKSTART.de.md).

## Steuerung

| Aktion | Desktop | Touch |
| --- | --- | --- |
| Raum bauen / auswählen | Klick auf ein Baufeld | Baufeld antippen |
| Kamera verschieben | Ziehen, WASD oder Pfeiltasten | Mit einem Finger ziehen |
| Zoom | Mausrad oder − / + | Pinch oder − / + |
| Raum drehen | R oder „Drehen“ | „Drehen“ |
| Kamera drehen | Q / E oder „Ansicht“ | „Ansicht“ |
| Bauwerkzeug verlassen | Esc oder „Erkunden“ | „Erkunden“ |
| Zeit anhalten / starten | Leertaste oder „Start/Pause“ | „Start/Pause“ |

## Räume

| Raum | Größe | Einmalig | Pro Tag | Freischaltung |
| --- | --- | ---: | ---: | --- |
| Flur | 1×1 | 80 € | 1 € / Feld | Sofort |
| Klassenzimmer | 4×3 | 3.600 € | 36 € | Sofort |
| Sanitärraum | 2×2 | 1.100 € | 14 € | Sofort |
| Lehrerzimmer | 3×2 | 1.800 € | 18 € | Sofort |
| Schulgarten | 3×3 | 800 € | 5 € | Sofort |
| Mensa | 4×3 | 3.000 € | 30 € | Förderziel 2 |
| Bibliothek | 4×3 | 4.800 € | 35 € | Förderziel 2 |
| Labor | 4×3 | 6.500 € | 45 € | Förderziel 4 |
| Atelier | 4×3 | 5.200 € | 34 € | Förderziel 4 |
| Sporthalle | 5×4 | 9.000 € | 60 € | Förderziel 6 |

Räume müssen direkt an einen Flur grenzen, der zum Eingang führt. Ein Rückbau zahlt die Hälfte der Baukosten zurück. Startflur und erstes Klassenzimmer bleiben erhalten; benötigte Verbindungsflure können nicht entfernt werden.

## Architektur

| Bereich | Aufgabe |
| --- | --- |
| `Assets/SchoolTycoon/Core` | Engineunabhängige C#-Simulation, Regeln, Finanzen und Wegsuche |
| `Assets/SchoolTycoon/Runtime` | Unity-Ansicht, Kamera, Figuren, Menüs und Speichern |
| `Assets/SchoolTycoon/Resources` | Eigener mitgelieferter Campus-Shader |
| `Assets/SchoolTycoon/Editor` | Build-Befehle und Szenenmenü |
| `Assets/SchoolTycoon/Tests/EditMode` | Tests für Finanzen, Bau, Wege, Speicherung und Fortschritt |
| `tools` | Prüfungen ohne Unity-Editor |

Grafik entsteht zur Laufzeit aus eigenen Formen; kostenpflichtige Assets werden nicht benötigt. Das Projekt verwendet die **Built-in Render Pipeline**, uGUI und den klassischen Input Manager. Die Startszene wird im Play-Modus durch `SchoolApp` aufgebaut.

## Builds und GitHub

**School Simulation → Build → Windows / Linux / Android APK / WebGL**. Das passende Build-Modul muss in Unity Hub installiert sein. Für Android werden SDK, NDK, OpenJDK und IL2CPP benötigt. Ausgabe: `Builds/Android/TheSchoolSimulation-0.3.0.apk`.

`Simulation checks` führt nach Push oder Pull Request die strukturellen Prüfungen und dieselben Domain-Tests mit .NET 8 aus. Der manuelle Workflow `Unity build (licensed runner)` verwendet einen selbst gehosteten, aktivierten Unity-Runner mit dem Label `unity` und der Umgebungsvariable `UNITY_EDITOR`. Er setzt einen solchen Runner voraus und startet keinen unbeaufsichtigten Cloud-Build ohne Editor.

## Prüfung der Unity-Anwendung

Die Domain-Prüfungen decken die ursprüngliche Kampagne und die neuen Abläufe ab: ein komplett ausgebauter Campus mit allen Forschungsprojekten und AGs sowie 660 Schultage mit Abschlüssen und begrenzten Historien. **Unity-Import, tatsächliche Menübedienung, Rendering und Plattform-Builds müssen noch im Unity-Editor geprüft werden.** Eine strukturierte Prüfliste steht in [PLAYTEST.md](docs/PLAYTEST.md).

Die Figurenansicht verwendet eigene prozedurale Formen und zeigt höchstens 64 anwesende Schüler und 24 Teammitglieder. Alle Schüler werden unabhängig davon vollständig simuliert. Figuren verwenden Flurwege, besitzen aber keine gegenseitige Kollisionsvermeidung. Wetter und Jahreszeiten sind vereinfacht; es gibt keine Tonkulisse. WebGL-Speicherpersistenz muss auf der Zielplattform geprüft werden.

Konzept: **KoSch / The School Simulation**. Bezug auf das ausgewählte School-Simulation-Plugin; eigenständige Umsetzung in Unity. Noch keine Veröffentlichungslizenz gewählt. Unity und Unity-Pakete behalten ihre jeweiligen Bedingungen.
