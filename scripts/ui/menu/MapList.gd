extends ScrollContainer

var mapButtonTemplate = preload("res://prefabs/map_button.tscn")

@onready var container := $"Container"

func _ready() -> void:
	pass
	#BuildList()

func BuildMap() -> void:
	pass
	#var mapButton := mapButtonTemplate.instantiate()

func BuildList() -> void:
	print("need user folder constant")
