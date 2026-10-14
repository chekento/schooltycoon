extends SceneTree

func _initialize() -> void:
	call_deferred("configure")

func configure() -> void:
	while EditorInterface.get_resource_filesystem().is_scanning():
		await process_frame
	var settings := EditorInterface.get_editor_settings()
	settings.set_setting("export/android/android_sdk_path", OS.get_environment("ANDROID_HOME"))
	settings.set_setting("export/android/java_sdk_path", OS.get_environment("JAVA_HOME"))
	settings.set_setting("export/android/debug_keystore", ProjectSettings.globalize_path("res://Build/debug.keystore"))
	settings.set_setting("export/android/debug_keystore_user", "androiddebugkey")
	settings.set_setting("export/android/debug_keystore_pass", "android")
	var version := Engine.get_version_info()
	var templates := OS.get_data_dir().path_join("godot/export_templates/%s.%s.%s.stable.mono" % [version.major, version.minor, version.patch])
	var presets := ConfigFile.new()
	presets.load("res://export_presets.cfg")
	presets.set_value("preset.0.options", "custom_template/debug", templates.path_join("android_debug.apk"))
	presets.set_value("preset.0.options", "custom_template/release", templates.path_join("android_release.apk"))
	presets.set_value("preset.0.options", "keystore/debug", "res://Build/debug.keystore")
	presets.set_value("preset.0.options", "keystore/debug_user", "androiddebugkey")
	presets.set_value("preset.0.options", "keystore/debug_password", "android")
	# The optimized preview uses the same public development identity as 0.6.
	presets.set_value("preset.0.options", "keystore/release", "res://Build/debug.keystore")
	presets.set_value("preset.0.options", "keystore/release_user", "androiddebugkey")
	presets.set_value("preset.0.options", "keystore/release_password", "android")
	presets.save("res://export_presets.cfg")
	print("Using official Android template: ", templates, " exists=", FileAccess.file_exists(templates.path_join("android_debug.apk")))
	var filename := OS.get_config_dir().path_join("godot/editor_settings-%s.%s.tres" % [version.major, version.minor])
	var result := ResourceSaver.save(settings, filename)
	if result != OK:
		push_error("Could not save Android editor settings")
		quit(1)
		return
	quit()
