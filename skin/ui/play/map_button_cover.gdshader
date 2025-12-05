shader_type canvas_item;

void fragment() {
	COLOR = vec4(COLOR.rgb, UV.x * 2.0 * COLOR.a);
}
