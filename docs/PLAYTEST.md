# Unity-Prüfliste für 0.3.0

Diese Prüfungen benötigen den Unity-Editor und wurden in der Quellumgebung noch nicht ausgeführt. Die separat ausgeführten Domain-Tests ersetzen diese Bedienungs-, Darstellungs- und Plattformprüfungen nicht.

## Play Mode

| Ablauf | Erwartetes Verhalten |
| --- | --- |
| Projekt mit 6000.0.65f1 importieren | Keine C#-, Shader- oder Paketfehler; `Campus.unity` öffnet sich |
| Neues Spiel, Charakter → Haar → Name | Je Schritt Auswahlmöglichkeiten; eigene Namenseingabe und drei Vorschläge; sichtbare Schulleitung |
| Lehrkraft einstellen | Erste Klasse automatisch betreut; Förderziel einmal bezahlt; Figuren gehen ins eigene Klassenzimmer |
| Raum bauen, drehen, zurückbauen | Vorschau, Kosten und Flurverbindungen stimmen; einmalige Erstattung; geschützte Startbereiche bleiben |
| Alle acht Verwaltungstabs öffnen | Inhalte scrollbar, keine überlagerten Schaltflächen; Deutsch und Englisch lesbar |
| Zweite Klasse bauen und Lehrkraft einstellen | Klasse und Personen korrekt zugeordnet; Klassenname und eigener Stundenplan bedienbar |
| Lehrkraft in andere Klasse wechseln | Alte Klasse unbesetzt, Finanzierung folgt tatsächlicher Betreuung, Zuordnung nicht doppelt |
| Schülerprofil, Klassenwechsel und Fördermaßnahmen | Fachwerte sichtbar, Platzgrenzen eingehalten, Maßnahme nur einmal je Tag bezahlt |
| Nur ein Fach unterrichten | Andere Fächer steigen ohne AG oder besondere Maßnahme nicht durch Unterricht |
| Tagesablauf laufen lassen | 08–12 Unterricht, 12 Uhr Mensa, 13–15 Unterricht, ab 15 Uhr AG / Erholung; Wege durch Flure |
| Lehrkraft fortbilden | Kompetenz / Moral steigen, Gehalt +5 €/Tag, maximal drei Stufen |
| Raumzustand senken, reparieren und ausbauen | Zustand, Einmalkosten, Plätze / Boni und laufende Kosten stimmen; Ausbau sichtbar |
| Forschung starten und mehrere Tage abwarten | Fortschritt und Abschluss sichtbar; Kosten einmalig; keine zweite parallele Forschung |
| Letzte Bibliothek entfernen und wieder aufbauen | Forschung pausiert und setzt später fort |
| AG starten, pausieren, wieder starten | Ausstattung erhalten, tägliche Kosten nur aktiv mit vorhandenem Raum, Figuren besuchen AGs |
| Ereignis öffnen, auch mit negativem Geld | Drei Wege und Auswirkungen sichtbar; kostenlose Wahl verfügbar; kein doppelter Tagesabschluss |
| Tag 20, 40 und 60 abschließen | Zeugnisse und Tagesberichte lesbar; Versetzung / Wiederholung am Jahresende |
| Jahrgang 6 erfolgreich abschließen | Nur Abschlussjahrgang verlässt Schule; andere Profile erhalten; Neuaufnahme möglich |
| Speichern, Play stoppen, erneut starten | Räume, Profile, Pläne, Forschung, AGs und Zeugnisse identisch wiederhergestellt |
| Gültigen v2-Stand laden | Bestehende Schule übernommen, Profile ergänzt, alte Datei erhalten, später v3 gespeichert |
| Hauptstand beschädigen | Gültige Sicherung bevorzugt; Fehlermeldung bei Wiederherstellung oder Neustart |
| Neues Spiel bestätigen | Neuer Campus und Charakterdesigner, keine Restdaten der alten Schule |

## Darstellung und Zielplattformen

Prüfe Desktop 1600×900 und 1280×720, Android im Querformat mit Aussparungen sowie unterschiedliche Seitenverhältnisse. Kontrolliere Scrollen, Schaltflächen, Modalfenster, lange deutsche Texte, Avatar-Eingabe, Maus- und Touchgesten. Die UI wird skaliert; die Mindestgröße und Bedienbarkeit müssen am Gerät bestätigt werden.

Erstelle Windows, Linux, Android und WebGL mit den vorhandenen Build-Befehlen. Auf Android: APK installieren, Start, Pausieren, Fortsetzen, Speichern, erneutes Öffnen und Spielleistung mit ausgebautem Campus prüfen. Auf WebGL: Speicherung nach vollständigem Schließen und Öffnen des Browsers gesondert prüfen. Es liegt noch kein getestetes Build-Artefakt vor.
