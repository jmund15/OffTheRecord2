shader_type canvas_item;
uniform float width : hint_range(-1.0,5.0);
uniform vec4 outline_color;
//uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 size = vec2(width) / vec2(textureSize(TEXTURE, 0));
   
    vec4 sprite_color = texture(TEXTURE, UV);
   
    float alpha = sprite_color.a;
    alpha += texture(TEXTURE, UV + vec2(0.0, -size.y)).a;
    alpha += texture(TEXTURE, UV + vec2(size.x, -size.y)).a;
    alpha += texture(TEXTURE, UV + vec2(size.x, 0.0)).a;
    alpha += texture(TEXTURE, UV + vec2(size.x, size.y)).a;
    alpha += texture(TEXTURE, UV + vec2(0.0, size.y)).a;
    alpha += texture(TEXTURE, UV + vec2(-size.x, size.y)).a;
    alpha += texture(TEXTURE, UV + vec2(-size.x, 0.0)).a;
    alpha += texture(TEXTURE, UV + vec2(-size.x, -size.y)).a;
   
    vec3 final_color = mix(outline_color.rgb, sprite_color.rgb, sprite_color.a);
    COLOR = vec4(final_color, clamp(alpha, 0.0, 1.0));
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
