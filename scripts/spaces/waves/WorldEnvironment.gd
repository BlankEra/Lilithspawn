extends WorldEnvironment

var centerOffset : Vector2

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		centerOffset = event.position - get_viewport().get_visible_rect().size / 2

func _process(delta: float) -> void:
	environment.sky_rotation.y += delta * centerOffset.x / 50000
