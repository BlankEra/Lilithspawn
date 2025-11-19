extends Button

@onready var importDialog := $"../../../../ImportDialog"

func _pressed() -> void:
	importDialog.show()
