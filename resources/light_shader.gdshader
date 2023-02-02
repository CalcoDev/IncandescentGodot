shader_type canvas_item;

void fragment() {
	COLOR = vec4(1.0, 1.0, 1.0, 0.0);
	
	if (AT_LIGHT_PASS)
		COLOR.a = 1.0;
}
