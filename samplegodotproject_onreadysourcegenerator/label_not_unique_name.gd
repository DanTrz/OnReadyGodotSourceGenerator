extends Label

@onready var myparent = get_parent()

func _ready() -> void:
	if myparent != null:
		printt("Get Parent = " , str(myparent.name), " GdScript Code ")
	else:
		printt("Get Parent = NULL / ERROR / - GdScript Code ")
