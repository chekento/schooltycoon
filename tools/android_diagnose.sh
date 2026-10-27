#!/usr/bin/env bash
set -uo pipefail
bash tools/android_smoke.sh &
smoke=$!
(
  while true; do
    date -u >> android-process-state.log
    pid=$(adb shell pidof cloud.kosch.rektor.godot | tr -d '\r')
    if [ -n "$pid" ]; then
      adb shell ps -T -p "$pid" -o PID,TID,NAME >> android-process-state.log 2>&1 || true
      adb shell dumpsys meminfo cloud.kosch.rektor.godot | tail -25 >> android-process-state.log 2>&1 || true
    fi
    sleep 2
  done
) &
monitor=$!
trap 'kill "$monitor" "$smoke" 2>/dev/null || true' EXIT
wait "$smoke"
status=$?
exit "$status"
