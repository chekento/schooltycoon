# Spielsysteme 0.2.0

## Tagesablauf

Der Schultag läuft von 08:00 bis 17:00. Sechs Unterrichtsblöcke beginnen um 08, 09, 10, 11, 13 und 14 Uhr. Um 12 Uhr gehen die dargestellten Figuren in die Mensa, sofern vorhanden. Ab 15 Uhr bevorzugen sie den Garten. Fehlende Einrichtungen führen die Figuren zum Klassenzimmer zurück. Die Unterrichtsergebnisse werden am Tagesende für die gesamte Schule berechnet.

## Kapazität und Einnahmen

Ein Klassenraum bietet 24 physische Plätze. Betreute Plätze sind `min(Klassenräume, Lehrkräfte) × 24`. Die Bildungsförderung beträgt 28 € pro tatsächlich betreutem Schüler und Tag. Die Mensa generiert 3 € pro versorgtem Schüler, höchstens 72 pro Mensa. Neue Anmeldungen nach Tagesende werden erst am folgenden Tag abgerechnet.

## Kosten

Bau- und Einstellungskosten sind einmalig. Lehrkräfte kosten bei Einstellung 700 €, Hausdienst und Beratung 400 €. Die Tagesgehälter stehen in den Bewerbungen. Lernmaterial kostet pro Schüler und Tag 2 / 4 / 6 € je nach Budget. Flure kosten pro Feld 1 € am Tag, andere Räume den angegebenen festen Betrag. Rückbau erstattet 50 % der Baukosten.

## Schulqualität

- Sauberkeit sinkt durch Nutzung; Hausdienst wirkt dem entgegen.
- Lernerfolg berücksichtigt Lehrerkompetenz, betreute Plätze, Fächervielfalt, passende Fachlehrkräfte, Fachräume und Lernmaterial.
- Zufriedenheit berücksichtigt Sanitärversorgung, Essen, Sauberkeit, Unterrichtsplätze, Garten und Schulberatung.
- Guter Lernerfolg und Zufriedenheit steigern das Ansehen; stark unzufriedene Schulen verlieren Ansehen.
- Zufriedenheit und Ansehen beeinflussen Anmeldungen. Neue Schüler belegen nur betreute freie Plätze. Fehlende Betreuung kann zu Abmeldungen führen.

## Förderziele

| Nr. | Voraussetzung | Fördergeld | Freischaltung danach |
| --- | --- | ---: | --- |
| 1 | Eine Lehrkraft | 1.400 € | — |
| 2 | Sanitärraum und Lehrerzimmer | 2.200 € | Mensa, Bibliothek, Grundstückskauf |
| 3 | Zwei Klassenräume und zwei Lehrkräfte | 3.400 € | — |
| 4 | Mensa und Bibliothek | 5.000 € | Labor, Atelier |
| 5 | 48 Schüler und 65 Zufriedenheit | 5.500 € | — |
| 6 | Labor, Atelier und 70 Lernerfolg | 6.500 € | Sporthalle |
| 7 | 96 Schüler, Sporthalle und 75 Ansehen | 9.000 € | Freies Weiterspielen |

Förderziele werden in dieser Reihenfolge erfüllt. Mehrere bereits erfüllte Ziele werden gemeinsam ausgezahlt; jedes nur einmal. Auf Tag 4, 8, 12 usw. wartet ein Ereignis. Es hat drei Entscheidungen; die kostenlose Variante ist auch bei Schulden möglich. Bis zur Antwort ruht die Simulation.

## Grenzen des Prototyps

Es gibt noch keine individuellen Noten, Klassen- oder Personalzuordnung und keine physische Personen-Kollisionsvermeidung. Die Figurenzahl wird in der Ansicht begrenzt; die Wirtschaftsberechnung verwendet die vollständige Schülerzahl. Nicht jeder Fachraum ist zwingend: Fachräume verbessern den Lernerfolg. Diese Version hat noch keine Tonkulisse, Jahreszeiten oder Forschung.

Bei weniger als −5.000 € oder fünf aufeinanderfolgenden Tagen mit Schulden endet die Schule. Eine neue Schule ist danach möglich.
