#version 450

layout(set = 0, binding = 1) uniform texture2D Texture;
layout(set = 0, binding = 2) uniform sampler Sampler;
layout(set = 0, binding = 3) uniform ColorTintBuffer
{
    vec4 ColorTint;
};

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 0) out vec4 OutColor;

void main()
{
    OutColor = texture(sampler2D(Texture, Sampler), fsin_TexCoords) * ColorTint;
}
