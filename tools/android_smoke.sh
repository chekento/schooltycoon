#!/usr/bin/env bash
set -euo pipefail
mkdir -p captures
adb install -r Builds/Android/Rektor-0.6.0-godot.apk
adb logcat -c
adb shell am start -W -n cloud.kosch.rektor.godot/com.godot.game.GodotApp
sleep 5
adb exec-out screencap -p > captures/android-menu.png
adb shell am force-stop cloud.kosch.rektor.godot
adb logcat -c
adb shell am start -W -n cloud.kosch.rektor.godot/com.godot.game.GodotApp --esa command_line_params '--,--smoke-test'
for attempt in {1..120}; do
  adb logcat -d > android-smoke.log
  if grep -q 'GODOT_SMOKE_PASS' android-smoke.log; then
    grep 'GODOT_SMOKE_PASS' android-smoke.log
    exit 0
  fi
  if grep -Eq 'GODOT_SMOKE_FAIL|FATAL EXCEPTION|Fatal signal' android-smoke.log; then
    tail -100 android-smoke.log
    exit 1
  fi
  sleep 1
done
tail -100 android-smoke.log
echo 'Android scene smoke checks timed out' >&2
exit 1
