#!/usr/bin/env bash
set -euo pipefail
mkdir -p captures
package=cloud.kosch.rektor.godot
activity="$package/com.godot.game.GodotApp"
logfile=android-smoke.log
failures='GODOT_SMOKE_FAIL|REKTOR_TOUCH_FAIL|REKTOR_ACTION_FAIL|FATAL EXCEPTION|Fatal signal|oom-kill:.*ch.rektor.godot'

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
adb install --no-incremental -r Builds/Android/Rektor-0.7.0-godot.apk

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
adb shell am force-stop "$package"
adb logcat -c
adb shell am start -W -n "$activity" --esa command_line_params '--,--smoke-test'
wait_log 'GODOT_SMOKE_PASS 38 checks'
grep 'GODOT_SMOKE_PASS' "$logfile" > android-domain-result.log

# Drive actual GUI hit-testing with Android input, not emitted button signals.
adb shell am force-stop "$package"
adb logcat -c
adb shell am start -W -n "$activity" --esa command_line_params '--,--touch-check'
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
