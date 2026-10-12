# Rektor · Android-Build

Der Android-Einstieg, Touch-Bedienung, Safe Area, zuklappbare Menüs, Grafikstufen, reduzierte Bewegung und Speicherung sind implementiert. **Eine fertige APK liegt noch nicht vor.**

## Festgestellter Blocker

Der offizielle Unity-Editor 6000.0.65f1 wurde installiert und der tatsächliche Android-Build aufgerufen. Die Verbindung zum LicenseClient scheiterte; Unity meldete `Licensing initialization failed`. Die erneute Prüfung des GitHub-Builds fand ebenfalls keine vollständige Unity-Aktivierung. Es wurde kein APK-Artefakt hochgeladen.

Der Android-Workflow bricht jetzt mit einem Fehler ab, wenn die erforderlichen Aktivierungs-Secrets fehlen. Im Job und seiner Zusammenfassung stehen die fehlenden **Namen**, ohne deren Werte auszugeben. Ein erfolgreicher APK-Job setzt einen erfolgreichen Unity-Build, die APK-Prüfung und den Upload voraus.

## Einmalig für GitHub einrichten

Für eine vorhandene Unity-Personal-Lizenz:

1. In **Unity Hub → Preferences → Licenses → Add → Get a free personal license** mit dem eigenen Unity-Konto aktivieren.
2. Die von Unity erzeugte `Unity_lic.ulf` suchen:
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
3. Unter [schooltycoon → Settings → Secrets and variables → Actions](https://github.com/chekento/schooltycoon/settings/secrets/actions) diese drei Repository-Secrets anlegen:

   | Secret | Inhalt |
   |---|---|
   | `UNITY_LICENSE` | Vollständiger Inhalt der aktivierten `.ulf`-Datei |
   | `UNITY_EMAIL` | E-Mail des zugehörigen Unity-Kontos |
   | `UNITY_PASSWORD` | Passwort dieses Unity-Kontos |

4. [Actions → Rektor Android APK](https://github.com/chekento/schooltycoon/actions/workflows/android-apk.yml) öffnen und **Run workflow → main → Run workflow** wählen. Alternativ den fehlgeschlagenen Job erneut starten.
5. Nach einem erfolgreichen Lauf im Bereich **Artifacts** `Rektor-0.5.0-Android-APK` herunterladen, das ZIP entpacken und `Rektor-0.5.0.apk` auf dem Android-Gerät installieren.

Für eine vorhandene professionelle Lizenz mit Seriennummer kann `UNITY_SERIAL` an die Stelle von `UNITY_LICENSE` treten; die beiden Konto-Secrets bleiben erforderlich. Die persönliche Aktivierung über Unity Hub und die CI-Einrichtung sind in der [GameCI-Anleitung](https://game.ci/docs/github/activation/) beschrieben. Unity Personal unterstützt keine manuelle Aktivierung über eine selbst erzeugte `.alf`-Anforderung; maßgeblich sind die [Unity-Aktivierungsmethoden](https://docs.unity3d.com/6000.0/Documentation/Manual/LicenseActivationMethods.html).

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
