#version 450

layout(set = 0, binding = 0) uniform WorldViewProjBuffer
{
    mat4 WorldViewProj;
};

layout(set = 0, binding = 1) uniform WorldBuffer
{
    mat4 World;
};

layout(set = 0, binding = 2) uniform EnvironmentBuffer
{
    vec4 ViewPosition;
	vec4 LightPos0;
	vec4 LightPos1;
	vec4 LightPos2;
	vec4 LightPos3;
};

layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 TexCoords;
layout (location = 2) in vec3 Normal;
layout (location = 3) in vec3 Tangent;

layout (location = 0) out vec2 fsin_TexCoords;
layout (location = 1) out vec3 fsin_WorldPos;
layout (location = 2) out vec3 fsin_Normal;
layout (location = 3) out vec3 fsin_TangentWorldPos;
layout (location = 4) out vec3 fsin_TangentViewPos;
layout (location = 5) out vec3 fsin_TangentLightPos0;
layout (location = 6) out vec3 fsin_TangentLightPos1;
layout (location = 7) out vec3 fsin_TangentLightPos2;
layout (location = 8) out vec3 fsin_TangentLightPos3;

void main()
{
    fsin_TexCoords = TexCoords;
	fsin_Normal = mat3(World) * Normal;
	fsin_WorldPos = vec3(World * vec4(Position, 1.0));

	mat3 normalMatrix = transpose(inverse(mat3(World)));
    vec3 T = normalize(normalMatrix * Tangent);
    vec3 N = normalize(normalMatrix * Normal);
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);
    
    mat3 TBN = transpose(mat3(T, B, N));    
    fsin_TangentLightPos0 = TBN * LightPos0.xyz;
	fsin_TangentLightPos1 = TBN * LightPos1.xyz;
	fsin_TangentLightPos2 = TBN * LightPos2.xyz;
	fsin_TangentLightPos3 = TBN * LightPos3.xyz;
    fsin_TangentViewPos   = TBN * ViewPosition.xyz;
    fsin_TangentWorldPos  = TBN * fsin_WorldPos;

    gl_Position = WorldViewProj * vec4(Position, 1.0);
}
