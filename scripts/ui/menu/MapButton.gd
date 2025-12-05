extends Panel

@onready var title := $"Title"
@onready var extra := $"Extra"
@onready var cover := $"Cover"
@onready var button := $"Button"
@onready var outline_shader := $"Outline".material as ShaderMaterial

var map: String	# the parsed map which the button represents (change to map type in the future)
var index := 0	# global map list index
var hovered := false
var selected := false
var sizeHeight := 80.0			# vertical size config (update in MapList properties)
var hoverSizeOffset := 10.0
var selectedSizeOffset := 20.0
var sizeOffset := 0.0			# applied vertical size offset
var centerOffset := 0.0			# normalized distance from maplist center
var stickoutOffset := 0.0		# horizontal offset when selected
var targetOutlineFill := 0.0
var outlineFill := 0.0

func _ready() -> void:
	outline_shader = outline_shader.duplicate()
	$"Outline".material = outline_shader
	
	button.mouse_entered.connect(Hover.bind(true))
	button.mouse_exited.connect(Hover.bind(false))
	button.pressed.connect(Select)

func _process(delta: float) -> void:
	stickoutOffset = lerp(stickoutOffset, 0.05 if selected else 0.0, min(1, 16 * delta))
	anchor_left = 0.1 - centerOffset / 20 - stickoutOffset
	size = (Vector2(size.x, lerp(size.y, sizeHeight + sizeOffset, min(1, 16 * delta))))
	outlineFill = lerp(outlineFill, targetOutlineFill, min(1, 10 * delta))
	
	outline_shader.set_shader_parameter("fill", outlineFill)

func Hover(hover: bool) -> void:
	hovered = hover
	sizeOffset = (hoverSizeOffset if hovered else 0.0) + (selectedSizeOffset if selected else 0.0)
	create_tween().set_trans(Tween.TRANS_QUAD).tween_property(self, "self_modulate", Color(Color(0.1, 0.025, 0.05) if hovered else Color.BLACK, 0.875), 0.15)

func Select(select: bool = true) -> void:
	if selected and select:
		print("play map %s" % map)
	
	selected = select
	sizeOffset = (hoverSizeOffset if hovered else 0.0) + (selectedSizeOffset if selected else 0.0)
	create_tween().set_trans(Tween.TRANS_QUAD).tween_property(cover, "modulate", Color.WHITE if selected else Color(1, 1, 1, 0.5), 0.1)

func Deselect() -> void:
	Select(false)

func UpdateInfo(newMap: String) -> void:
	map = newMap
	name = map
	#title.text = map
	#extra.text = "[color=808080]extra"

func UpdateOutline(targetFill: float, fill: float = -1.0) -> void:
	targetOutlineFill = targetFill
	
	if fill != -1.0:
		outlineFill = fill
