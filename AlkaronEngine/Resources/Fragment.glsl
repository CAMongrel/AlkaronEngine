#version 450

layout(set = 0, binding = 0) uniform texture2D Texture;
layout(set = 0, binding = 1) uniform sampler Sampler;

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 0) out vec4 OutColor;

void main()
{
    //OutColor = texture(sampler2D(Texture, Sampler), fsin_TexCoords);
	OutColor = vec4(fsin_TexCoords.x, fsin_TexCoords.y, 0, 1);
}
