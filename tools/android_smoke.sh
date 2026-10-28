#!/usr/bin/env bash
set -euo pipefail
mkdir -p captures
package=cloud.kosch.rektor.godot
activity=""
logfile=android-smoke.log
failures='GODOT_SMOKE_FAIL|REKTOR_TOUCH_FAIL|REKTOR_ACTION_FAIL|FATAL EXCEPTION|Fatal signal|FORTIFY:|oom-kill:.*ch.rektor.godot'

capture_failure() {
  local status=$?
  if [ "$status" -ne 0 ]; then
    sleep 3
    adb logcat -b all -d > "$logfile" || true
    adb shell dumpsys dropbox --print data_app_native_crash > android-crash-report.log || true
    if [ -n "$activity" ] && ! check_alive; then
      adb shell am start -W -n "$activity" > android-recovery.log 2>&1 || true
      sleep 5
      adb logcat -d | grep 'REKTOR_PREVIOUS_NATIVE' >> android-recovery.log || true
    fi
    grep -E 'godot|FORTIFY|Fatal signal|DOTNET|mono|crash' "$logfile" | tail -100 || true
  fi
  exit "$status"
}
trap capture_failure EXIT

wait_log() {
  local pattern="$1"
  for attempt in {1..180}; do
    adb logcat -d > "$logfile"
    if grep -Eq "$failures" "$logfile"; then tail -100 "$logfile"; return 1; fi
    if grep -Eq "$pattern" "$logfile"; then return 0; fi
    sleep 1
  done
  tail -100 "$logfile"
  echo "Android checkpoint timed out: $pattern" >&2
  return 1
}
check_alive() {
  adb shell pidof "$package" | tr -d '\r' | grep -q '[0-9]'
}
tap_public() {
  local id="$1"
  wait_log "REKTOR_UI $id [0-9]+ [0-9]+"
  local position tap_x tap_y
  position=$(awk -v id="$id" '$0 ~ "REKTOR_UI " id " " {print $(NF-1),$NF}' "$logfile" | tail -1 | tr -d '\r')
  read -r tap_x tap_y <<< "$position"
  echo "Tap public $id at $tap_x $tap_y"
  adb logcat -c
  adb shell input tap "$tap_x" "$tap_y"
}
adb install --no-incremental -r Builds/Android/Rektor-0.7.0-godot.apk
# A fresh test device must acknowledge Android's fullscreen tutorial before GUI taps.
adb shell settings put secure immersive_mode_confirmations confirmed
activity=$(adb shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER "$package" | tr -d '\r' | tail -1)
case "$activity" in "$package"/*) echo "Launcher component: $activity" ;; *) echo 'No exported launcher activity found' >&2; exit 1 ;; esac

# Cold launcher starts must stay open rather than merely display the splash screen.
for launch in 1 2 3; do
  adb shell am force-stop "$package"
  adb logcat -c
  adb shell am start -W -n "$activity"
  wait_log 'REKTOR_READY menu'
  sleep 3
  check_alive
  echo "PASS normal cold launch $launch"
done
adb exec-out screencap -p > captures/android-menu.png
tap_public diagnostics
tap_public selfCheck
wait_log 'GODOT_SMOKE_PASS 40 checks'
grep 'GODOT_SMOKE_PASS' "$logfile" > android-domain-result.log

# Drive actual GUI hit-testing with Android input, not emitted button signals.
adb shell am force-stop "$package"
adb logcat -c
adb shell am start -W -n "$activity"
tap_public diagnostics
tap_public touchCheck
for target in campaign new1 difficulty1 gender2 hair0 directorDone storyTeam hire0 menu campaign continue1; do
  wait_log "REKTOR_TOUCH $target [0-9]+ [0-9]+"
  position=$(awk -v id="$target" '$0 ~ "REKTOR_TOUCH " id " " {print $(NF-1),$NF}' "$logfile" | tail -1 | tr -d '\r')
  read -r tap_x tap_y <<< "$position"
  # Clear the previous instruction so a repeated campaign button cannot reuse stale coordinates.
  adb logcat -c
  adb shell input tap "$tap_x" "$tap_y"
done
wait_log 'REKTOR_TOUCH_PASS'
grep 'REKTOR_TOUCH_PASS' "$logfile" > android-touch-result.log
check_alive
adb exec-out screencap -p > captures/android-campus.png

# Exercise Android pause/resume and check that the same game process survives.
before=$(adb shell pidof "$package" | tr -d '\r')
adb shell input keyevent KEYCODE_HOME
sleep 2
adb shell am start -W -n "$activity"
wait_log 'REKTOR_STAGE campus-resumed'
after=$(adb shell pidof "$package" | tr -d '\r')
test "$before" = "$after"
sleep 10
check_alive
adb shell dumpsys meminfo "$package" > android-memory.log
adb logcat -d > "$logfile"
if grep -Eq "$failures" "$logfile"; then tail -100 "$logfile"; exit 1; fi
echo 'ANDROID_LIFECYCLE_PASS cold launches, 11 real taps, save/continue, background/resume and idle'
