extends TextureRect

var skinManager = SkinManager
var settingsManager = SettingsManager

var mousePosition := Vector2.ZERO

func _ready() -> void:
	skinManager.OnLoaded.connect(UpdateTexture)
	settingsManager.Settings.FieldUpdated.connect(func(field: String, _value: Variant) -> void:
		if field == "CursorScale": UpdateSize()
	)
	
	UpdateTexture()
	UpdateSize()

func _process(_delta: float) -> void:
	position = mousePosition - size / 2

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		mousePosition = event.position

func UpdateTexture() -> void:
	texture = skinManager.Skin.CursorImage

func UpdateSize() -> void:
	size = Vector2.ONE * 32 * settingsManager.Settings.CursorScale
