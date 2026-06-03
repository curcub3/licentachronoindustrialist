extends PanelContainer

signal task_toggled(task_id: String, is_completed: bool)

const TaskChecklistItemScene := preload("res://Assets/UI/components/TaskChecklistItem.tscn")

var _items := {}
var _title_label: Label
var _progress_label: Label
var _scroll: ScrollContainer
var _list: VBoxContainer

func _ready() -> void:
	focus_mode = Control.FOCUS_NONE
	_build()
	refresh()


func add_task(task_id: String, title: String, description: String = "", is_completed: bool = false, is_optional: bool = false, priority: int = 0) -> void:
	if not is_node_ready():
		await ready

	if _items.has(task_id):
		var existing = _items[task_id]
		existing.setup(task_id, title, description, is_completed, is_optional, priority)
	else:
		var item = TaskChecklistItemScene.instantiate()
		_list.add_child(item)
		_items[task_id] = item
		item.task_toggled.connect(_on_item_toggled)
		item.setup(task_id, title, description, is_completed, is_optional, priority)
	refresh()


func set_title(title: String) -> void:
	if not is_node_ready():
		await ready
	if _title_label != null:
		_title_label.text = title


func remove_task(task_id: String) -> void:
	if not _items.has(task_id):
		return
	var item = _items[task_id]
	_items.erase(task_id)
	item.queue_free()
	refresh()


func set_task_completed(task_id: String, is_completed: bool) -> void:
	if not _items.has(task_id):
		return
	_items[task_id].set_completed(is_completed)
	refresh()


func clear_tasks() -> void:
	for item in _items.values():
		item.queue_free()
	_items.clear()
	refresh()


func refresh() -> void:
	if _progress_label == null:
		return
	var total := _items.size()
	var complete := 0
	for item in _items.values():
		if item.is_completed:
			complete += 1
	_progress_label.text = "%d/%d finalizate" % [complete, total]


func _build() -> void:
	if get_child_count() > 0:
		return

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 8)
	margin.add_theme_constant_override("margin_top", 6)
	margin.add_theme_constant_override("margin_right", 8)
	margin.add_theme_constant_override("margin_bottom", 6)
	add_child(margin)

	var root := VBoxContainer.new()
	root.add_theme_constant_override("separation", 6)
	margin.add_child(root)

	var header := HBoxContainer.new()
	header.add_theme_constant_override("separation", 8)
	root.add_child(header)

	_title_label = Label.new()
	_title_label.text = "Următoarea acțiune"
	_title_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_title_label.clip_text = true
	header.add_child(_title_label)

	_progress_label = Label.new()
	header.add_child(_progress_label)

	_scroll = ScrollContainer.new()
	_scroll.custom_minimum_size = Vector2(0, 172)
	_scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	root.add_child(_scroll)

	_list = VBoxContainer.new()
	_list.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_list.add_theme_constant_override("separation", 3)
	_scroll.add_child(_list)


func _on_item_toggled(task_id: String, is_completed: bool) -> void:
	refresh()
	task_toggled.emit(task_id, is_completed)
