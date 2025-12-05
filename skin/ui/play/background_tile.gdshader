shader_type canvas_item;

void fragment() {
	vec2 uv = UV;
	uv.x -= TIME;

	COLOR = texture(TEXTURE, uv);
}