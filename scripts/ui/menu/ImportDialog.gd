extends FileDialog

func _files_selected(paths: PackedStringArray) -> void:
	MapParser.BulkImport(paths)
