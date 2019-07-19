#version 450

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 0) out vec4 OutColor;

void main()
{
    OutColor = vec4(fsin_TexCoords, 0, 1);
}
