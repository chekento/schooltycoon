# Rektor · vorbereitete Funktionen in Godot 0.7

Godot verwendet den vollständigen vorhandenen Simulationskern. Diese Übersicht beschreibt die tatsächlichen Einstiege im Spiel.

| Anforderung | Umsetzung und Einstieg |
|---|---|
| REKTOR, Hauptmenü und Spielstände | Sechs getrennte Plätze, Fortsetzen, bestätigtes Ersetzen, Sicherungen, Prüfsummen, Import/Export und Zugriff auf den zuletzt gespielten Platz. |
| Drei Schwierigkeiten | Entspannt, Ausgewogen und Anspruchsvoll bei einer neuen Schule. Gleiche Regeln wie Unity mit 720-Tage-Prüfungen; bestehende Schulen behalten ihre Schwierigkeit. |
| Sandbox | Eigener Menüeinstieg mit unendlich Geld. Wege, Klassenplätze und Lehrkräfte bleiben Teil des Betriebs. |
| Charakterdesigner | Nacheinander Geschlecht, Haarfarbe und Name. |
| Deutsche, englische und US-Schulen | 146 Fächer einschließlich 43 AP-Kursen, regionale Wochenplan-Vorlagen und bis zu 200 eigene Wahlfächer unter **Verwalten → Klassen & Stundenpläne**. Umfang und Quellen: [Curriculum](CURRICULUM.md). |
| Klassen und Fachräume | 17 Raumtypen, mehrere unabhängige Klassen, Lehrkräfte, 30 Wochenstunden pro Klasse und Fachraumreservierungen. |
| Schultürme | Sechs Geschosse, tragende Grundflächen, übereinanderliegende Treppen und Flurwege. **Ansicht / Mara → Wie baue ich Schultürme?** erklärt den Aufbau. |
| Raum-Upgrades | 57 Item-Typen, fünf Raumstufen, einzelne Käufe mit Voraussetzungen und täglichen Kosten. Die Kaufansicht zeigt Wirkungen auf Lernen, Freude, Wohlbefinden und Sauberkeit. Dekoration folgt nach funktionaler Ausstattung. |
| Führung und Langzeitspiel | Zwölf Mara-Kapitel mit situativen Ratschlägen und vollständiger Kapitelübersicht unter **Ansicht / Mara → Schulvision**. Zeit, Forschung, Personal und Schulentwicklung steuern Freischaltungen. |
| Schüler | Profile, Kursnoten, Förderung, Stress, Anwesenheit, Prüfungen, Versetzung und Abschlüsse mit wählbarem Abschlussjahrgang 6–13. |
| Personal und Wirtschaft | Einstellen, Zuordnung, Freistellen und Fortbildung; Gehälter, Material, Versorgung, Instandhaltung, Aufnahme- und Förderpolitik, Tagesprognose und Überbrückungshilfe. |
| Weitere Aktivitäten | Vier Forschungsrichtungen, fünf AGs, zwölf Ereignisse, Jahresprojekte, Zeugnisse, Prestige und Erfolge. |
| Grafik und Bedienung | Abgerundete 3D-Modelle, sichtbare Ausstattung, Figuren, Glas, Pflanzen und illustrierter Menühintergrund. Ziehen, Pinch-Zoom, Drehen, Etagenwechsel und einklappbare Menüs. UI-Tipps lösen keine Bauaktionen darunter aus. |
| Einstellungen | Deutsch/Englisch, Musik, Effekte, Kamera, Grafikstufen, 30/60 FPS, reduzierte Bewegung, Raumnamen und Autosave. Android startet sparsam. |
| Android-Stabilität | Später 3D-Aufbau, wiederverwendete Räume/Figuren, geringere Renderlast, bestätigtes Beenden, Speichern beim Hintergrundwechsel, Sitzungsfortsetzung und kopierbare lokale Startdiagnose. |
| Unity später weiterführen | Originalprojekt am Repository-Root unverändert; zusätzlich **archive/unity-0.5**. Gemeinsamer C#-Kern und Spielstand-Schema 5. |

## Prüfgrenzen

Godot 0.7 besteht lokal 38 Szenenprüfungen, einschließlich fünf Fortsetzungszyklen, gezieltem Raum-Neuaufbau und einem sechsgeschossigen Campus. Die Android-Freigabe verlangt drei Kaltstarts, 38 Prüfungen innerhalb der APK, elf echte Bildschirm-Tipps und Hintergrund/Fortsetzen auf Android 11 und 16 mit jeweils 2 GB Emulator-RAM. Physische Samsung-/ARM64-Geräte können hier nicht direkt geprüft werden. Bei einem weiteren Abbruch liefert **Hilfe / Startdiagnose → Diagnose kopieren** die letzte erreichte Phase und verwaltete Fehlermeldungen.
