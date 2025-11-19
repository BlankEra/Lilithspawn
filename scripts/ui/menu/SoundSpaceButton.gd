extends Button

@onready var label := $"RichTextLabel"

func _mouse_entered() -> void:
	label.text = "[center][color=ffffff40]Inspired by [color=ffffffff]Sound Space"

func _mouse_exited() -> void:
	label.text = "[center][color=ffffff40]Inspired by Sound Space"

func _pressed() -> void:
	OS.shell_open("https://www.roblox.com/games/2677609345")
