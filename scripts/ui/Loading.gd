extends Control

var SceneManager = load("res://scripts/SceneManager.cs")

@onready var background := $"Background"
@onready var splash := $"Splash"
@onready var splashShift := $"SplashShift"

func _ready() -> void:
	var inTween := create_tween().set_trans(Tween.TRANS_QUAD).set_parallel()
	inTween.tween_property(background, "color", Color("#060509"), 1)
	inTween.tween_property(splash, "modulate", Color.WHITE, 0.5)
	inTween.tween_property(splashShift, "modulate", Color.WHITE, 0.25)
	inTween.set_trans(Tween.TRANS_SINE)
	inTween.chain().tween_property(splashShift, "modulate", Color.TRANSPARENT, 2.5)
	
	inTween.chain().tween_callback(func():
		var outTween := create_tween().set_trans(Tween.TRANS_QUAD)
		outTween.tween_property(background, "color", Color.BLACK, 0.5)
		outTween.tween_property(splash, "modulate", Color.BLACK, 0.5)
		outTween.tween_callback(func():
			SceneManager.Load("res://scenes/main_menu.tscn", false)
		)
	)
