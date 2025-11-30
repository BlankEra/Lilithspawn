extends Panel

@export var buttonSize := 80.0
@export var buttonHoverOffset := 20.0
@export var spacing := 12.0
@export var scrollStep := 1500.0
@export var scrollFriction := 20.0
@export var scrollElasticity := 0.02

@onready var mask := $"Mask"
@onready var scrollBar := $"ScrollBar/Main"
@onready var selectionCursor := $"SelectionCursor"

var maps: Array[String] = [] # get this from db in the future

var mapButtonTemplate = preload("res://prefabs/map_button.tscn")
var mapButtons: Dictionary[String, Panel] = {}
var mapButtonCache: Array[Panel] = []
var hoveredButton: Panel

var scrollLength := 0.0
var scrollMomentum := 0.0
var scroll := 0.0
var targetScroll := 0.0
var mouseScroll := false
var displaySelectionCursor := false

func _ready() -> void:
	mouse_exited.connect(ToggleSelectionCursor.bind(false))
	
	for i in range(20):
		maps.append("map %s" % i)

func _process(delta: float) -> void:
	var mapCount := maps.size()
	var scrollElasticOffset := 0.0
	
	if (targetScroll <= 0 and scrollMomentum < 0) or (targetScroll >= scrollLength and scrollMomentum > 0):
		scrollElasticOffset = scrollMomentum * scrollElasticity
	
	scrollLength = max(0, mapCount * (buttonSize + spacing) - spacing - size.y) + buttonHoverOffset
	scrollMomentum = lerp(scrollMomentum, 0.0, min(1, scrollFriction * delta))
	
	if mouseScroll:
		targetScroll = lerp(targetScroll, scrollLength * clamp(inverse_lerp(position.y + scrollBar.size.y / 2, position.y + size.y - scrollBar.size.y / 2, get_viewport().get_mouse_position().y), 0, 1), min(1, 24 * delta))
	else:
		targetScroll = clamp(targetScroll + scrollMomentum * delta, 0, scrollLength) + scrollElasticOffset
	
	scroll = lerp(scroll, targetScroll, min(1, 20 * delta))
	
	scrollBar.anchor_top = max(0, (targetScroll - scrollElasticOffset) / (scrollLength + size.y))
	scrollBar.anchor_bottom = min(1, scrollBar.anchor_top + size.y / (scrollLength + size.y))
	
	var drawnButtons: Array[Panel] = []
	var buttonSizeOffsets: Array[float] = []
	var upOffset := 0.0
	var downOffset := 0.0
	
	for i in range(mapCount):
		var map := maps[i]	# use map ID here
		var offset := i * (buttonSize + spacing)
		var top := offset - scroll
		var bottom := top + buttonSize
		var display := top < size.y and bottom > 0
		var button: Panel = mapButtons.get(map)
		
		# cache/ignore if out of map list
		if !display:
			if button != null:
				mapButtons.erase(map)
				mapButtonCache.append(button)
				mask.remove_child(button)
				
				if button == hoveredButton:
					hoveredButton = null
			
			continue
		
		# we know everything must be rendered from here
		if button == null:
			button = mapButtonCache.pop_front()
			
			if button == null:
				button = mapButtonTemplate.instantiate()
				button.sizeHeight = buttonSize
				button.hoverSizeOffset = buttonHoverOffset
				button.mouse_entered.connect(func() -> void:
					hoveredButton = button
					ToggleSelectionCursor(true)
				)
			
			button.index = i
			mapButtons[map] = button
			mask.add_child(button)
			button.UpdateInfo(map)
		
		var sizeOffset: float = button.size.y - button.sizeHeight
		
		downOffset += sizeOffset
		drawnButtons.append(button)
		buttonSizeOffsets.append(sizeOffset)
	
	for i in range(drawnButtons.size()):
		var button := drawnButtons[i]
		var sizeOffset := buttonSizeOffsets[i]
		
		var isFirst: bool = button.index == 0
		var isLast: bool = button.index == mapCount - 1
		
		var indexOffset: float = button.index * (buttonSize + spacing)
		var top := indexOffset - scroll - sizeOffset / 2
		
		downOffset -= sizeOffset
		top += (upOffset - downOffset) / 2 + buttonHoverOffset / 2
		upOffset += sizeOffset
		
		# normalized offset from list center
		var centerOffset: float = abs((top + button.size.y / 2) - size.y / 2) / (size.y / 2 + buttonSize / 2)
		centerOffset = cos(PI * centerOffset / 2)
		
		button.z_index = 1 if isFirst or isLast else 0
		button.position = Vector2(button.position.x, top)
		button.anchor_left = 0.05 - centerOffset / 20
	
	var selectionCursorPosition := Vector2(
		hoveredButton.position.x - 60 if hoveredButton != null else -80.0,
		hoveredButton.position.y + hoveredButton.size.y / 2 - selectionCursor.size.y / 2 if hoveredButton != null else selectionCursor.position.y
	)
	
	selectionCursor.position = lerp(selectionCursor.position, selectionCursorPosition, min(1, 12 * delta))
	selectionCursor.rotation = -selectionCursor.position.y / 60

func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		match event.button_index:
			MOUSE_BUTTON_RIGHT:
				mouseScroll = event.pressed
			MOUSE_BUTTON_WHEEL_DOWN:
				scrollMomentum += scrollStep
			MOUSE_BUTTON_WHEEL_UP:
				scrollMomentum -= scrollStep

func ToggleSelectionCursor(display: bool) -> void:
	if display == displaySelectionCursor:
		return
	
	displaySelectionCursor = display
	
	if display and hoveredButton:
		selectionCursor.position.y = hoveredButton.position.y
	
	var tween := create_tween().set_parallel().set_trans(Tween.TRANS_QUAD)
	tween.tween_property(selectionCursor, "modulate", Color.WHITE if display else Color.TRANSPARENT, 0.1)
