extends WorldEnvironment

func _process(delta: float) -> void:
	# don't run this if playing (waiting on new runner)
	var viewport := get_viewport()
	var centerOffset := viewport.get_mouse_position() - viewport.get_visible_rect().size / 2
	
	environment.sky_rotation.y += delta * centerOffset.x / 50000
