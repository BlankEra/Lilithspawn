extends ColorRect

var settingsManager = SettingsManager

var menuShown := false
var settingPanels: Dictionary[String, Panel] = {}
@onready var deselect := $"Deselect"
@onready var holder := $"Holder"
@onready var sidebar := $"Holder/Sidebar/Container"
@onready var categories := $"Holder/Categories"
@onready var selectedCategory := $"Holder/Categories/Gameplay"

func _ready() -> void:
	settingsManager.OnShown.connect(ShowMenu)
	settingsManager.Settings.FieldUpdated.connect(func(field: String, value: Variant) -> void:
		var setting: Panel = settingPanels.get(field)
		
		if setting == null:
			return
		
		var slider := setting.find_child("HSlider")
		var lineEdit := setting.find_child("LineEdit")
		var toggle := setting.find_child("CheckButton")
		
		if slider != null:
			UpdateSlider(slider, lineEdit, value)
		elif toggle != null:
			UpdateToggle(toggle, value)
	)
	
	deselect.pressed.connect(func() -> void: settingsManager.ShowMenu(false))
	
	for buttonHolder: ColorRect in sidebar.get_children():
		var category := categories.find_child(buttonHolder.name)
		
		if category != null:
			buttonHolder.get_node("Button").pressed.connect(SelectCategory.bind(category))
	
	for category: ScrollContainer in categories.get_children():
		for setting: Panel in category.get_node("Container").get_children():
			settingPanels[setting.name] = setting
			
			var value: Variant = settingsManager.Settings.get(setting.name)
			
			if value == null:
				continue
			
			var slider := setting.find_child("HSlider")
			var lineEdit := setting.find_child("LineEdit")
			var toggle := setting.find_child("CheckButton")
			
			if slider != null:
				SetupSlider(setting.name, slider, lineEdit)
				UpdateSlider(slider, lineEdit, value)
			elif toggle != null:
				SetupToggle(setting.name, toggle)
				UpdateToggle(toggle, value)

func SetupSlider(setting: String, slider: HSlider, lineEdit: LineEdit) -> void:
	var applyLineEdit = func() -> void:
		var value := (lineEdit.placeholder_text if lineEdit.text == "" else lineEdit.text).to_float()
		print(value)
		settingsManager.ApplySetting(setting, value as Variant)
	
	lineEdit.text_submitted.connect(func(_text: String) -> void: applyLineEdit.call())
	lineEdit.focus_exited.connect(applyLineEdit)
	slider.value_changed.connect(func(value: float) -> void:
		settingsManager.ApplySetting(setting, value as Variant)
	)

func UpdateSlider(slider: HSlider, lineEdit: LineEdit, value: float) -> void:
	lineEdit.text = str(value)
	lineEdit.release_focus()
	slider.set_value_no_signal(value)

func SetupToggle(setting: String, button: CheckButton) -> void:
	button.toggled.connect(func(value: bool) -> void:
		settingsManager.ApplySetting(setting, value as Variant)
	)

func UpdateToggle(button: CheckButton, value: bool) -> void:
	button.button_pressed = value

func ShowMenu(shown: bool) -> void:
	menuShown = shown
	
	deselect.mouse_filter = MouseFilter.MOUSE_FILTER_STOP if menuShown else MouseFilter.MOUSE_FILTER_IGNORE
	
	move_to_front()
	
	if menuShown:
		visible = true
		holder.offset_top = 25
		holder.offset_bottom = 25
	
	var tween := create_tween().set_parallel().set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	tween.tween_property(self, "modulate", Color.WHITE if menuShown else Color.TRANSPARENT, 0.25)
	tween.tween_property(holder, "offset_top", 0 if menuShown else 25, 0.25)
	tween.tween_property(holder, "offset_bottom", 0 if menuShown else 25, 0.25)
	tween.chain().tween_callback(func(): visible = menuShown)

func SelectCategory(category: ScrollContainer) -> void:
	var sidebarButton := sidebar.get_node(NodePath(selectedCategory.name))
	sidebarButton.color = Color.TRANSPARENT
	selectedCategory.visible = false
	
	selectedCategory = category
	
	selectedCategory.visible = true
	sidebarButton = sidebar.get_node(NodePath(selectedCategory.name))
	sidebarButton.color = Color(1, 1, 1, 8.0/255)
