# Unity-Schnellstart

## Projekt öffnen

ZIP entpacken, in Unity Hub den Ordner mit `Assets`, `Packages` und `ProjectSettings` hinzufügen. Unity **6000.0.65f1** installieren oder einen kompatiblen Unity-6.0-Patch verwenden; ein Versionswechsel muss im Editor erneut geprüft werden. Die Pakete uGUI 2.0.0 und Test Framework 1.6.0 importieren lassen.

`Assets/Scenes/Campus.unity` öffnen und Play drücken. Dass die Szene vor Play leer ist, ist beabsichtigt: `SchoolApp.Boot` erzeugt Campus, Licht, Kamera, Figuren und Menüs zur Laufzeit.

## Erste fünf Minuten

1. Charakter, Haarfarbe und Namen wählen. Deine Schule bleibt zunächst pausiert.
2. Rechts **Team** öffnen und eine Lehrkraft einstellen. Das erste Förderziel zahlt 1.400 € aus.
3. Links **Sanitärraum** wählen. Links neben dem Startflur, bei Baufeld `(10,8)`, ist Platz dafür.
4. **Lehrerzimmer** wählen. Unterhalb des Startflurs, bei `(12,6)`, ist Platz. Das zweite Förderziel schaltet Mensa und Bibliothek frei.
5. Ein zweites Klassenzimmer passt bei `(16,10)`, neben dem ersten. Stelle eine zweite Lehrkraft ein.
6. Eine Mensa passt bei `(18,6)`. Ein Garten kann etwa bei `(15,5)` entstehen. Bei jedem Bau prüft die Vorschau Grundstück, Belegung und Fluranschluss.
7. **Erkunden** wählen und **Start** drücken. Ein Tag dauert bei 1× ungefähr 90 Sekunden. Mit **Tag beenden** kannst du direkt abrechnen.

Die Koordinaten dienen der Orientierung für Entwickler; das Spiel benötigt keine Koordinateneingabe. Beim Bauen sind grüne Felder gültig und rote ungültig. Räume können direkt nach dem ersten Förderziel gebaut werden, solange sie bereits freigeschaltet sind und das Geld reicht. Flure lassen sich Feld für Feld verlängern.

## Android-APK erstellen

Unity Hub: Android Build Support einschließlich SDK / NDK Tools und OpenJDK installieren. Im Projekt **School Simulation → Build → Android APK** wählen. Unity gibt die APK unter `Builds/Android/TheSchoolSimulation-0.2.0.apk` aus. Diese erste Fassung ist kein veröffentlichter oder auf einem Android-Gerät getesteter Build.

## Projekt von GitHub holen

Das Projekt liegt im Repository **chekento/schooltycoon**. Den ZIP-Download unter **Code → Download ZIP** entpacken oder mit Git klonen:

```sh
git clone https://github.com/chekento/schooltycoon.git
```

Danach den Ordner `schooltycoon` in Unity Hub hinzufügen. Änderungen lassen sich mit Git committen und in dasselbe Repository pushen.

## Speichern

Gespeichert wird lokal als `school-v2.json` im von Unity bereitgestellten `Application.persistentDataPath`. Der vorherige Stand wird als `.bak` aufbewahrt. Ein ungültiger Stand wird nicht übernommen; eine gültige Sicherung wird bevorzugt. Nach Bau, Personaländerungen, Ereignissen und Tagesende wird automatisch gespeichert.

Desktop- und Android-Speicherung sind implementiert; WebGL benötigt noch einen geprüften IndexedDB-Abgleich. Für eine Veröffentlichung sollten sämtliche Zielplattformen und die Bedienung auf echten Geräten geprüft werden.
