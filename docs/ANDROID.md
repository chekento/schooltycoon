# Rektor · Android-Build

Der Android-Einstieg, Touch-Bedienung, Safe Area, zuklappbare Menüs, Grafikstufen, reduzierte Bewegung und Speicherung sind implementiert. Eine **fertige APK wurde in dieser Sitzung nicht erzeugt**.

## Festgestellter Blocker

Der offizielle Unity-Editor 6000.0.65f1 wurde installiert und der tatsächliche Android-Build aufgerufen. Die Verbindung zum LicenseClient scheiterte; Unity meldete `Licensing initialization failed`. Auch der GitHub-Workflow lief: Seine Aktivierungsprüfung fand keine vollständige Unity-Aktivierung und übersprang den Spieler-Build. Ein grüner Aktivierungs-Prüfjob ist deshalb kein Beleg für eine APK. Es wurde kein APK-Artefakt hochgeladen.

## Mit aktiviertem Unity bauen

1. Unity 6000.0.65f1 in Unity Hub aktivieren.
2. Android Build Support samt SDK/NDK und OpenJDK installieren.
3. Dieses Repository öffnen und **Rektor → Build → Android APK** wählen.
4. Ergebnis: `Builds/Android/Rektor-0.5.0.apk`.

Konfiguration: ARM64, IL2CPP, Android ab API 26, automatisches Target SDK und Querformat links/rechts. Die APK nutzt ohne eigenen Veröffentlichungsschlüssel die Unity-Entwicklungssignatur. Für eine Store-Veröffentlichung ist ein eigener Schlüssel erforderlich.

## GitHub Actions

`.github/workflows/android-apk.yml` ist ausführbar und verwendet `game-ci/unity-builder@v4` mit der eigenen Editor-Buildmethode. Dafür werden im Repository eine gültige aktivierte Lizenzdatei als `UNITY_LICENSE` oder ein gültiger `UNITY_SERIAL` sowie `UNITY_EMAIL` und `UNITY_PASSWORD` als Actions-Secrets benötigt. Alternativ läuft der vorhandene manuelle Build-Workflow auf einem aktivierten selbst gehosteten Runner mit dem Label `unity` und `UNITY_EDITOR`.

Die Zugangsdaten gehören in GitHub Secrets. Der Workflow gibt nur aus, ob die Aktivierung vorhanden ist. Bei erfolgreichem Build prüft `tools/verify_apk.py` ZIP-Integrität, Manifest, DEX und den nativen ARM64-Unity/IL2CPP-Spieler und lädt die APK als Artifact hoch.

Referenz: https://game.ci/docs/github/builder/ · https://game.ci/docs/github/activation/

## Noch am Gerät zu prüfen

Installation und Start, Touch-Tap/Drag/Pinch, Bildschirmränder, Bildschirmrotation, App-Pause und Wiederaufnahme, Spielstand nach App-Neustart, Audioregler, Kamera und Bildrate im großen Campus. API-Kompilierung und Simulationstests können diese Geräteprüfungen nicht ersetzen.
