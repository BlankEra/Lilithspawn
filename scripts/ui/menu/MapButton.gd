extends Panel

@onready var title := $"Title"
@onready var extra := $"Extra"
@onready var button := $"Button"

var map	# the parsed map which the button represents
var index := 0	# global map list index
var sizeHeight := 80.0
var hoverSizeOffset := 20.0
var sizeOffset := 0.0

func _ready() -> void:
	button.mouse_entered.connect(Hover.bind(true))
	button.mouse_exited.connect(Hover.bind(false))
	button.pressed.connect(Press)

func _process(delta: float) -> void:
	set_size(Vector2(size.x, lerp(size.y, sizeHeight + sizeOffset, min(1, 16 * delta))))

func Hover(hovered: bool) -> void:
	sizeOffset = hoverSizeOffset if hovered else 0.0

func Press() -> void:
	print(map)

func UpdateInfo(newMap: String) -> void:
	map = newMap
	name = map
	title.text = map
	extra.text = "extra"
