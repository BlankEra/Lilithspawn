extends Control

@onready var menus := $"Menus"
@onready var currentMenu := $"Menus/Main"
@onready var lastMenu := currentMenu

func _ready() -> void:
	Input.mouse_mode = Input.MOUSE_MODE_HIDDEN
	
	for button: Button in $"Menus/Main/Buttons".get_children():
		var menu := menus.find_child(button.name, false)
		
		if menu != null:
			button.pressed.connect(Transition.bind(menu))

func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if !event.pressed:
			return
		
		match event.button_index:
			MOUSE_BUTTON_XBUTTON1:
				Transition($"Menus/Main")
			MOUSE_BUTTON_XBUTTON2:
				Transition(lastMenu)

func Transition(menu: Panel, instant: bool = false) -> void:
	if menu == currentMenu:
		return
	
	lastMenu = currentMenu
	currentMenu = menu
	
	var tweenTime := 0.0 if instant else 0.15
	
	var outTween := create_tween().set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	outTween.tween_property(lastMenu, "modulate", Color.TRANSPARENT, tweenTime)
	outTween.tween_callback(func() -> void: lastMenu.visible = false)
	outTween.play()
	
	menu.visible = true
	
	var inTween := create_tween().set_trans(Tween.TRANS_QUAD)
	inTween.tween_property(menu, "modulate", Color.WHITE, tweenTime)
	inTween.play()
