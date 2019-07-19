#version 450

layout(set = 0, binding = 0) uniform WorldViewProjBuffer
{
    mat4 WorldViewProj;
};

layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 TexCoords;
layout (location = 2) in vec3 Normal;
layout (location = 3) in vec3 Tangent;
layout (location = 4) in vec3 Bitangent;

layout (location = 0) out vec2 fsin_TexCoords;

void main()
{
    fsin_TexCoords = TexCoords;
    gl_Position = WorldViewProj * vec4(Position, 1);
}
