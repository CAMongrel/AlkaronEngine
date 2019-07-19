#version 450

// lights
//uniform vec3 lightPositions[4];
//uniform vec3 lightColors[4];

const float PI = 3.14159265359;

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 1) in vec3 fsin_WorldPos;
layout(location = 2) in vec3 fsin_Normal;

layout(set = 0, binding = 1) uniform CameraBuffer
{
    vec4 CameraPos;
};

layout(set = 0, binding = 2) uniform texture2D AlbedoTexture;
//layout(set = 0, binding = 2) uniform texture2D NormalTexture;
layout(set = 0, binding = 3) uniform texture2D MetallicRoughnessTexture;
//layout(set = 0, binding = 4) uniform texture2D RoughnessTexture;
//layout(set = 0, binding = 5) uniform texture2D AOTexture;

layout(set = 0, binding = 4) uniform sampler AlbedoSampler;
layout(set = 0, binding = 5) uniform sampler MetallicRoughnessSampler;

layout(location = 0) out vec4 OutColor;

// ----------------------------------------------------------------------------
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / max(denom, 0.001); // prevent divide by zero for roughness=0.0 and NdotH=1.0
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

// ----------------------------------------------------------------------------
void main()
{		
	vec3 albedo = texture(sampler2D(AlbedoTexture, AlbedoSampler), fsin_TexCoords).rgb;
	albedo = pow(albedo, vec3(2.2));
	vec2 metRough = texture(sampler2D(MetallicRoughnessTexture, MetallicRoughnessSampler), fsin_TexCoords).rg;
	float metallic = metRough.r;
	float roughness = metRough.g;
	float ao = 0.5f;

	vec3 lightPositions[4];
	vec3 lightColors[4];

	lightPositions[0] = vec3(10, 10, 10);
	lightColors[0] = vec3(300.0, 300.0, 300.0);
	lightPositions[1] = vec3(-10, -10, 10);
	lightColors[1] = vec3(300.0, 0.0, 300.0);

    vec3 N = normalize(fsin_Normal);
    vec3 V = normalize(CameraPos.xyz - fsin_WorldPos);

    // calculate reflectance at normal incidence; if dia-electric (like plastic) use F0 
    // of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow)    
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    // reflectance equation
    vec3 Lo = vec3(0.0);
    for(int i = 0; i < 2; ++i) 
    {
        // calculate per-light radiance
        vec3 L = normalize(lightPositions[i] - fsin_WorldPos);
        vec3 H = normalize(V + L);
        float distance = length(lightPositions[i] - fsin_WorldPos);
        float attenuation = 1.0 / (distance * distance);
        vec3 radiance = lightColors[i] * attenuation;

        // Cook-Torrance BRDF
        float NDF = DistributionGGX(N, H, roughness);   
        float G   = GeometrySmith(N, V, L, roughness);      
        vec3 F    = fresnelSchlick(clamp(dot(H, V), 0.0, 1.0), F0);
           
        vec3 nominator    = NDF * G * F; 
        float denominator = 4 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
        vec3 specular = nominator / max(denominator, 0.001); // prevent divide by zero for NdotV=0.0 or NdotL=0.0
        
        // kS is equal to Fresnel
        vec3 kS = F;
        // for energy conservation, the diffuse and specular light can't
        // be above 1.0 (unless the surface emits light); to preserve this
        // relationship the diffuse component (kD) should equal 1.0 - kS.
        vec3 kD = vec3(1.0) - kS;
        // multiply kD by the inverse metalness such that only non-metals 
        // have diffuse lighting, or a linear blend if partly metal (pure metals
        // have no diffuse light).
        kD *= 1.0 - metallic;	  

        // scale light by NdotL
        float NdotL = max(dot(N, L), 0.0);        

        // add to outgoing radiance Lo
        Lo += (kD * albedo / PI + specular) * radiance * NdotL;  // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
    }   
    
    // ambient lighting (note that the next IBL tutorial will replace 
    // this ambient lighting with environment lighting).
    vec3 ambient = vec3(0.03) * albedo * ao;

    vec3 color = ambient + Lo;

    // HDR tonemapping
    color = color / (color + vec3(1.0));
    // gamma correct
    color = pow(color, vec3(1.0/2.2)); 

    OutColor = vec4(color, 1.0);
}
