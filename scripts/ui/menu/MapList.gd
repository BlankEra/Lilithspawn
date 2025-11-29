extends Panel

@export var buttonSize := 80.0
@export var spacing := 12.0
@export var scrollStep := 500.0
@export var scrollFriction := 8.0
@export var scrollElasticity := 0.02

@onready var scrollBar := $"ScrollBar/Main"

var maps: Array[String] = [] # get this from db in the future

var mapButtonTemplate = preload("res://prefabs/map_button.tscn")
var mapButtons: Dictionary[String, Panel] = {}
var mapButtonCache: Array[Panel] = []

var scrollLength := 0.0
var scrollMomentum := 0.0
var scroll := 0.0
var mouseScroll := false

func _ready() -> void:
	for i in range(20):
		maps.append("map %s" % i)

func _process(delta: float) -> void:
	var mapCount := maps.size()
	var scrollElasticOffset := 0.0
	
	if (scroll <= 0 and scrollMomentum < 0) or (scroll >= scrollLength and scrollMomentum > 0):
		scrollElasticOffset = scrollMomentum * scrollElasticity
	
	scrollLength = max(0, mapCount * (buttonSize + spacing) - spacing - size.y)
	scrollMomentum = lerp(scrollMomentum, 0.0, min(1, scrollFriction * delta))
	
	if mouseScroll:
		scroll = lerp(scroll, scrollLength * clamp(inverse_lerp(position.y + scrollBar.size.y / 2, position.y + size.y - scrollBar.size.y / 2, get_viewport().get_mouse_position().y), 0, 1), min(1, 12 * delta))
	else:
		scroll = clamp(scroll + scrollMomentum * delta, 0, scrollLength) + scrollElasticOffset
	
	scrollBar.anchor_top = max(0, (scroll - scrollElasticOffset) / (scrollLength + size.y))
	scrollBar.anchor_bottom = min(1, scrollBar.anchor_top + size.y / (scrollLength + size.y))
	
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
				remove_child(button)
			
			continue
		
		# we know everything must be rendered from here
		# build button if missing
		if button == null:
			button = mapButtonCache.pop_front()
			
			if button == null:
				button = mapButtonTemplate.instantiate()
			
			mapButtons[map] = button
			
			add_child(button)
			SetupMapButton(button, map)
		
		# normalized offset from list center
		var centerOffset: float = abs(lerp(top, bottom, 0.5) - size.y / 2) / (size.y / 2 + buttonSize / 2)
		centerOffset = cos(PI * centerOffset / 2)
		
		button.position = Vector2(button.position.x, top)
		button.anchor_left = 0.05 - centerOffset / 20
		button.custom_minimum_size = Vector2(0, buttonSize)

func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		match event.button_index:
			MOUSE_BUTTON_RIGHT:
				mouseScroll = event.pressed
			MOUSE_BUTTON_WHEEL_DOWN:
				scrollMomentum += scrollStep
			MOUSE_BUTTON_WHEEL_UP:
				scrollMomentum -= scrollStep

func SetupMapButton(button: Panel, map: String) -> void:
	button.name = map
	
	button.get_node("Title").text = map
	button.get_node("Extra").text = "extra"
