extends Label

var frames := 0
var time := 0.0

func _process(delta: float) -> void:
	frames += 1
	time += delta
	
	if time >= 1:
		text = "%s FPS" % frames
		time -= 1
		frames = 0
