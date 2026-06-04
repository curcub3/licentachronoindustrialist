extends PanelContainer

signal task_toggled(task_id: String, is_completed: bool)

@export var task_id: String = ""
@export var title: String = ""
@export_multiline var description: String = ""
@export var is_completed: bool = false
@export var is_optional: bool = false
@export var priority: int = 0

var _checkbox: CheckBox
var _title_label: Label
var _meta_label: Label

func _ready() -> void:
	focus_mode = Control.FOCUS_ALL
	mouse_filter = Control.MOUSE_FILTER_STOP
	tooltip_text = description
	_build()
	refresh()


func setup(new_task_id: String, new_title: String, new_description: String, completed: bool, optional: bool, new_priority: int) -> void:
	task_id = new_task_id
	title = new_title
	description = new_description
	is_completed = completed
	is_optional = optional
	priority = new_priority
	if is_node_ready():
		refresh()


func set_completed(completed: bool) -> void:
	is_completed = completed
	if is_node_ready():
		refresh()


func refresh() -> void:
	tooltip_text = description
	if _checkbox != null:
		_checkbox.button_pressed = is_completed
	if _title_label != null:
		_title_label.text = title
		_title_label.modulate = Color(0.62, 0.68, 0.68, 1.0) if is_completed else Color(0.92, 0.94, 0.93, 1.0)
	if _meta_label != null:
		var parts: Array[String] = []
		if is_optional:
			parts.append("opțional")
		if priority > 0:
			parts.append("prioritate %d" % priority)
		_meta_label.text = " · ".join(parts)
		_meta_label.visible = parts.size() > 0
	modulate = Color(0.78, 0.82, 0.82, 1.0) if is_completed else Color.WHITE


func _build() -> void:
	if get_child_count() > 0:
		return

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 6)
	margin.add_theme_constant_override("margin_top", 3)
	margin.add_theme_constant_override("margin_right", 6)
	margin.add_theme_constant_override("margin_bottom", 3)
	add_child(margin)

	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 6)
	margin.add_child(row)

	_checkbox = CheckBox.new()
	_checkbox.custom_minimum_size = Vector2(24, 24)
	_checkbox.focus_mode = Control.FOCUS_ALL
	_checkbox.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
	_checkbox.toggled.connect(_on_checkbox_toggled)
	row.add_child(_checkbox)

	_title_label = Label.new()
	_title_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_title_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	row.add_child(_title_label)

	_meta_label = Label.new()
	_meta_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	row.add_child(_meta_label)


func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		_checkbox.button_pressed = not _checkbox.button_pressed


func _on_checkbox_toggled(pressed: bool) -> void:
	if is_completed == pressed:
		return
	is_completed = pressed
	refresh()
	task_toggled.emit(task_id, is_completed)
