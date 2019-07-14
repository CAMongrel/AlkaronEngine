#version 450

layout(set = 0, binding = 0) uniform WorldBuffer
{
    mat4 World;
};

layout (location = 0) in vec2 Position;
layout (location = 1) in vec2 TexCoords;
layout (location = 0) out vec2 fsin_TexCoords;

void main()
{
    fsin_TexCoords = TexCoords;
    gl_Position = World * vec4(Position, 0, 1);
	gl_Position.x = gl_Position.x * 2.0 - 1.0;
	gl_Position.y = (gl_Position.y * 2.0 - 1.0) * -1.0;
}
