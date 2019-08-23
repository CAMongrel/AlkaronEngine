using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using AlkaronEngine.Assets.Materials;
using Veldrid;

namespace AlkaronEngine.Graphics3D
{
    enum ConstructedShaderInputType
    {
        DiffuseAlbedo,
        Normal,
        AmbientOcclusion,
        Metallic,
        Roughness,
        MetallicRoughnessCombined,
        Emissive
    }

    enum ConstructedShaderInputValueType
    {
        Texture,
        ConstantValue
    }

    enum ConstructedShaderElementType
    {
        Vector4
    }

    enum BlendMode
    {
        Opaque,
        Mask,
        Blend
    }

    class ConstructedShaderInputElement
    {
        public ConstructedShaderInputType Type;
        public ConstructedShaderInputValueType ValueType;

        public string Name;
        public object Value;

        internal ConstructedShaderInputElement()
        {
            Name = "None";
            Value = (int)0;
        }
    }

    class ConstructedShaderInput
    {
        internal List<ConstructedShaderInputElement> Elements = new List<ConstructedShaderInputElement>();

        private int vertexShaderBindingCount;

        public ConstructedShaderInput(int setVertexShaderBindingCount)
        {
            vertexShaderBindingCount = setVertexShaderBindingCount;
        }

        internal int GetBindingLocation(ConstructedShaderInputType type)
        {
            int bindingLoc = vertexShaderBindingCount;
            for (int i = 0; i < Elements.Count; i++)
            {
                var elem = Elements[i];
                if (elem.Type == type)
                {
                    return bindingLoc;
                }

                bindingLoc += 1;
                if (elem.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    bindingLoc += 1;
                }
            }
            return -1;
        }

        internal void AddShaderInputs(StringBuilder sb)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                var elem = Elements[i];
                int bindingLoc = GetBindingLocation(elem.Type);
                if (bindingLoc == -1)
                {
                    continue;
                }

                if (elem.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    sb.AppendLine("layout(set = 0, binding = " + (bindingLoc) + ") uniform texture2D " + elem.Name + "Texture;");
                    sb.AppendLine("layout(set = 0, binding = " + (bindingLoc + 1) + ") uniform sampler " + elem.Name + "Sampler;");
                }
                else
                {
                    object val = elem.Value;

                    // TODO: Use optimized packing
                    sb.AppendLine("layout(set = 0, binding = " + (bindingLoc) + ") uniform " + elem.Name + "ConstantBuffer { vec4 " + elem.Name + "Constant; };");

                    /*if (val is float)
                    {
                        sb.AppendLine("layout(set = 0, binding = " + (bindingLoc) + ") uniform " + elem.Name + "ConstantBuffer { float " + elem.Name + "Constant; };");
                    }
                    else if (val is float[] fval)
                    {
                        switch (fval.Length)
                        {
                            case 2:
                                sb.AppendLine("layout(set = 0, binding = " + (bindingLoc) + ") uniform " + elem.Name + "ConstantBuffer { vec2 " + elem.Name + "Constant; };");
                                break;
                            case 3:
                                sb.AppendLine("layout(set = 0, binding = " + (bindingLoc) + ") uniform " + elem.Name + "ConstantBuffer { vec3 " + elem.Name + "Constant; };");
                                break;
                            case 4:
                                sb.AppendLine("layout(set = 0, binding = " + (bindingLoc) + ") uniform " + elem.Name + "ConstantBuffer { vec4 " + elem.Name + "Constant; };");
                                break;

                            default:
                                throw new NotImplementedException();
                        }                        
                    }*/
                }
            }
        }

        internal ConstructedShaderInputElement GetElementForType(ConstructedShaderInputType type)
        {
            return (from e in Elements
                    where e.Type == type
                    select e).FirstOrDefault();
        }

        internal void AddElementsToResourceLayoutDescription(List<ResourceLayoutElementDescription> description)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                var elem = Elements[i];

                if (elem.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    description.Add(new ResourceLayoutElementDescription(elem.Name + "Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));
                    description.Add(new ResourceLayoutElementDescription(elem.Name + "Sampler", ResourceKind.Sampler, ShaderStages.Fragment));
                }
                else
                {
                    description.Add(new ResourceLayoutElementDescription(elem.Name + "Constant", ResourceKind.UniformBuffer, ShaderStages.Fragment));
                }
            }
        }
    }

    class ConstructedShader
    {
        private static readonly string TexCoordsInputName = "in_TexCoords";
        private static readonly string WorldPosInputName = "in_WorldPos";
        private static readonly string NormalInputName = "in_Normal";
        private static readonly string TangentWorldPosInputName = "in_TangentWorldPos";
        private static readonly string TangentViewPosInputName = "in_TangentViewPos";
        private static readonly string TangentLightPos0InputName = "in_TangentLightPos0";
        private static readonly string TangentLightPos1InputName = "in_TangentLightPos1";
        private static readonly string TangentLightPos2InputName = "in_TangentLightPos2";
        private static readonly string TangentLightPos3InputName = "in_TangentLightPos3";

        private static readonly string ColorOutputName = "out_Color";

        public string Name { get; private set; }

        public BlendMode BlendMode { get; internal set; }
        public float AlphaCutoff { get; internal set; }

        public bool IsSkeletalShader { get; private set; }

        public Shader VertexShader { get; private set; }

        public ConstructedShaderInput Inputs { get; private set; }

        public ConstructedShader(string setName, Shader setVertexShader, bool setIsSkeletalShader)
        {
            IsSkeletalShader = setIsSkeletalShader;
            Name = setName;
            BlendMode = BlendMode.Opaque;
            AlphaCutoff = 0.0f;
            VertexShader = setVertexShader;
            Inputs = new ConstructedShaderInput(IsSkeletalShader ? 4 : 3);
        }

        internal ResourceLayoutDescription GetResourceLayoutDescription()
        {
            List<ResourceLayoutElementDescription> elements = new List<ResourceLayoutElementDescription>();
            // Vertex Shader
            elements.Add(new ResourceLayoutElementDescription("WorldViewProjBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));
            elements.Add(new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));
            elements.Add(new ResourceLayoutElementDescription("EnvironmentBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));
            if (IsSkeletalShader)
            {
                elements.Add(new ResourceLayoutElementDescription("JointMatricesBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex));
            }

            // Fragment Shader
            Inputs.AddElementsToResourceLayoutDescription(elements);

            return new ResourceLayoutDescription(elements.ToArray());
        }

        internal bool HasInputOfType(ConstructedShaderInputType type, ConstructedShaderInputValueType valueType)
        {
            var input = GetInputForType(type);
            return input != null && input.ValueType == valueType;
        }

        private ConstructedShaderInputElement? GetInputForType(ConstructedShaderInputType type)
        {
            return (from i in Inputs.Elements
                    where i.Type == type
                    select i).FirstOrDefault();
        }

        internal string GenerateShaderCode()
        {
            StringBuilder sb = new StringBuilder();
            AddHeader(sb);
            AddVertexShaderOutputs(sb);
            AddInputs(sb);
            AddFragmentShaderOutput(sb);
            AddFunctionLibrary(sb);
            AddMainMethod(sb);
            return sb.ToString();
        }

        private void AddHeader(StringBuilder sb)
        {
            sb.AppendLine("#version 450");
            sb.AppendLine();
        }

        private void AddVertexShaderOutputs(StringBuilder sb)
        {
            sb.AppendLine("layout (location = 0) in vec2 " + TexCoordsInputName + ";");
            sb.AppendLine("layout (location = 1) in vec3 " + WorldPosInputName + ";");
            sb.AppendLine("layout (location = 2) in vec3 " + NormalInputName + ";");
            sb.AppendLine("layout (location = 3) in vec3 " + TangentWorldPosInputName + ";");
            sb.AppendLine("layout (location = 4) in vec3 " + TangentViewPosInputName + ";");
            sb.AppendLine("layout (location = 5) in vec3 " + TangentLightPos0InputName + ";");
            sb.AppendLine("layout (location = 6) in vec3 " + TangentLightPos1InputName + ";");
            sb.AppendLine("layout (location = 7) in vec3 " + TangentLightPos2InputName + ";");
            sb.AppendLine("layout (location = 8) in vec3 " + TangentLightPos3InputName +";");
            sb.AppendLine();
        }

        private void AddInputs(StringBuilder sb)
        {
            Inputs.AddShaderInputs(sb);
            sb.AppendLine();
        }

        private void AddFragmentShaderOutput(StringBuilder sb)
        {
            sb.AppendLine("layout(location = 0) out vec4 " + ColorOutputName + ";");
            sb.AppendLine();
        }

        private void AddFunctionLibrary(StringBuilder sb)
        {
            sb.AppendLine("const float PI = 3.14159265359;");
            sb.AppendLine();
            sb.AppendLine("// ----------------------------------------------------------------------------");
            sb.AppendLine("float DistributionGGX(vec3 N, vec3 H, float roughness)");
            sb.AppendLine("{");
            sb.AppendLine("    float a = roughness*roughness;");
            sb.AppendLine("    float a2 = a*a;");
            sb.AppendLine("    float NdotH = max(dot(N, H), 0.0);");
            sb.AppendLine("    float NdotH2 = NdotH*NdotH;");
            sb.AppendLine("");
            sb.AppendLine("    float nom   = a2;");
            sb.AppendLine("    float denom = (NdotH2 * (a2 - 1.0) + 1.0);");
            sb.AppendLine("    denom = PI * denom * denom;");
            sb.AppendLine("");
            sb.AppendLine("    return nom / max(denom, 0.001); // prevent divide by zero for roughness=0.0 and NdotH=1.0");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine("// ----------------------------------------------------------------------------");
            sb.AppendLine("float GeometrySchlickGGX(float NdotV, float roughness)");
            sb.AppendLine("{");
            sb.AppendLine("    float r = (roughness + 1.0);");
            sb.AppendLine("    float k = (r*r) / 8.0;");
            sb.AppendLine("");
            sb.AppendLine("    float nom   = NdotV;");
            sb.AppendLine("    float denom = NdotV * (1.0 - k) + k;");
            sb.AppendLine("");
            sb.AppendLine("    return nom / denom;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine("// ----------------------------------------------------------------------------");
            sb.AppendLine("float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)");
            sb.AppendLine("{");
            sb.AppendLine("    float NdotV = max(dot(N, V), 0.0);");
            sb.AppendLine("    float NdotL = max(dot(N, L), 0.0);");
            sb.AppendLine("    float ggx2 = GeometrySchlickGGX(NdotV, roughness);");
            sb.AppendLine("    float ggx1 = GeometrySchlickGGX(NdotL, roughness);");
            sb.AppendLine("");
            sb.AppendLine("    return ggx1 * ggx2;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine("// ----------------------------------------------------------------------------");
            sb.AppendLine("vec3 fresnelSchlick(float cosTheta, vec3 F0)");
            sb.AppendLine("{");
            sb.AppendLine("    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        private void AddMainMethod(StringBuilder sb)
        {
            sb.AppendLine("void main()");
            sb.AppendLine("{");
            AddNormalCode(sb);
            sb.AppendLine("    vec3 V = normalize(" + TangentViewPosInputName + " - " + TangentWorldPosInputName + ");");
            AddLights(sb);
            AddLocalVars(sb);
            AddPbrCode(sb);
            AddShadow(sb);
            AddHDRGamma(sb);
            sb.AppendLine("    " + ColorOutputName + " = vec4(color, albedo.a);");
            //sb.AppendLine("    " + ColorOutputName + " = albedo;");
            sb.AppendLine("}");
        }

        private void AddShadow(StringBuilder sb)
        {
            //
        }

        private void AddHDRGamma(StringBuilder sb)
        {
            sb.AppendLine("    // HDR tonemapping");
            sb.AppendLine("    color = color / (color + vec3(1.0));");
            sb.AppendLine("    // gamma correct");
            sb.AppendLine("    color = pow(color, vec3(1.0/2.2));");
            sb.AppendLine();
        }

        private void AddPbrCode(StringBuilder sb)
        {
            sb.AppendLine("    // calculate reflectance at normal incidence; if dia-electric (like plastic) use F0 ");
            sb.AppendLine("    // of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow)");
            sb.AppendLine("    vec3 F0 = vec3(0.04);");
            sb.AppendLine("    F0 = mix(F0, albedo.rgb, metallic);");
            sb.AppendLine("");
            sb.AppendLine("    // reflectance equation");
            sb.AppendLine("    vec3 Lo = vec3(0.0);");
            sb.AppendLine("    for(int i = 0; i < 1; ++i) ");
            sb.AppendLine("    {");
            sb.AppendLine("        // calculate per-light radiance");
            sb.AppendLine("        vec3 L = normalize(lightPositions[i] - " + TangentWorldPosInputName + ");");
            sb.AppendLine("        vec3 H = normalize(V + L);");
            sb.AppendLine("        float distance = length(lightPositions[i] - " + TangentWorldPosInputName + ");");
            sb.AppendLine("        float attenuation = 1.0 / (distance * distance);");
            //sb.AppendLine("        float attenuation = 1.0 / distance;");
            sb.AppendLine("        vec3 radiance = lightColors[i] * attenuation;");
            sb.AppendLine("");
            sb.AppendLine("        // Cook-Torrance BRDF");
            sb.AppendLine("        float NDF = DistributionGGX(N, H, roughness);");
            sb.AppendLine("        float G   = GeometrySmith(N, V, L, roughness);");
            sb.AppendLine("        vec3 F    = fresnelSchlick(clamp(dot(H, V), 0.0, 1.0), F0);");
            sb.AppendLine("");
            sb.AppendLine("        vec3 nominator    = NDF * G * F; ");
            sb.AppendLine("        float denominator = 4 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);");
            sb.AppendLine("        vec3 specular = nominator / max(denominator, 0.001); // prevent divide by zero for NdotV=0.0 or NdotL=0.0");
            sb.AppendLine("");
            sb.AppendLine("        // kS is equal to Fresnel");
            sb.AppendLine("        vec3 kS = F;");
            sb.AppendLine("        // for energy conservation, the diffuse and specular light can't");
            sb.AppendLine("        // be above 1.0 (unless the surface emits light); to preserve this");
            sb.AppendLine("        // relationship the diffuse component (kD) should equal 1.0 - kS.");
            sb.AppendLine("        vec3 kD = vec3(1.0) - kS;");
            sb.AppendLine("        // multiply kD by the inverse metalness such that only non-metals ");
            sb.AppendLine("        // have diffuse lighting, or a linear blend if partly metal (pure metals");
            sb.AppendLine("        // have no diffuse light).");
            sb.AppendLine("        kD *= 1.0 - metallic;");
            sb.AppendLine("");
            sb.AppendLine("        // scale light by NdotL");
            sb.AppendLine("        float NdotL = max(dot(N, L), 0.0);");
            sb.AppendLine("");
            sb.AppendLine("        // add to outgoing radiance Lo");
            sb.AppendLine("        Lo += (kD * albedo.rgb / PI + specular) * radiance * NdotL;  // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    // ambient lighting (note that the next IBL tutorial will replace ");
            sb.AppendLine("    // this ambient lighting with environment lighting).");
            sb.AppendLine("    vec3 ambient = vec3(0.03) * albedo.rgb * ao;");
            sb.AppendLine("");
            sb.AppendLine("    vec3 color = ambient + Lo;");
            var emissiveInput = GetInputForType(ConstructedShaderInputType.Emissive);
            if (emissiveInput != null)
            {
                sb.AppendLine("    color += emissive;");
            }
            sb.AppendLine("");
        }

        private void AddLocalVars(StringBuilder sb)
        {
            var albedoInput = GetInputForType(ConstructedShaderInputType.DiffuseAlbedo);
            if (albedoInput != null)
            {
                if (albedoInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    sb.AppendLine("    vec4 albedo = texture(sampler2D(" + albedoInput.Name + "Texture, " + albedoInput.Name + "Sampler), " + TexCoordsInputName + ");");
                }
                else if (albedoInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                {
                    sb.AppendLine("    vec4 albedo = " + albedoInput.Name + "Constant;");
                }
            }
            else
            {
                sb.AppendLine("    vec4 albedo = vec3(1.0, 0.0, 1.0, 1.0);");
            }
            sb.AppendLine("    albedo.xyz = pow(albedo.xyz, vec3(2.2));");

            if (BlendMode == BlendMode.Mask)
            {
                sb.AppendLine("    if (albedo.a <= " + AlphaCutoff.ToString(CultureInfo.InvariantCulture) + ")");
                sb.AppendLine("    {");
                sb.AppendLine("        discard;");
                sb.AppendLine("    }");
            }

            var metRoughInput = GetInputForType(ConstructedShaderInputType.MetallicRoughnessCombined);
            if (metRoughInput != null)
            {
                if (metRoughInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    sb.AppendLine("    vec2 metRough = texture(sampler2D(" + metRoughInput.Name + "Texture, " + metRoughInput.Name + "Sampler), " + TexCoordsInputName + ").bg;");
                }
                else
                {
                    throw new NotImplementedException();
                }

                sb.AppendLine("    float metallic = metRough.x;");
                sb.AppendLine("    float roughness = metRough.y;");
            }
            else
            {
                var metInput = GetInputForType(ConstructedShaderInputType.Metallic);
                if (metInput != null)
                {
                    if (metInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                    {
                        sb.AppendLine("    float metallic = " + metInput.Name + "Constant.x;");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }                    
                }
                else
                {
                    sb.AppendLine("    float metallic = 1.0;");
                }
                var roughInput = GetInputForType(ConstructedShaderInputType.Roughness);
                if (roughInput != null)
                {
                    if (roughInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                    {
                        sb.AppendLine("    float roughness = " + roughInput.Name + "Constant.x;");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    sb.AppendLine("    float roughness = 1.0;");
                }
            }
            var aoInput = GetInputForType(ConstructedShaderInputType.AmbientOcclusion);
            if (aoInput != null)
            {
                if (aoInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    sb.AppendLine("    float ao = texture(sampler2D(" + aoInput.Name + "Texture, " + aoInput.Name + "Sampler), " + TexCoordsInputName + ").r;");
                }
                else
                {
                    sb.AppendLine("    float ao = " + aoInput.Name + "Constant.x;");
                }
            }
            else
            {
                sb.AppendLine("    float ao = 0.0;");
            }
            var emissiveInput = GetInputForType(ConstructedShaderInputType.Emissive);
            if (emissiveInput != null)
            {
                if (emissiveInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    sb.AppendLine("    vec3 emissive = texture(sampler2D(" + emissiveInput.Name + "Texture, " + emissiveInput.Name + "Sampler), " + TexCoordsInputName + ").rgb;");
                }
                else
                {
                    sb.AppendLine("    vec3 emissive = vec3(" + emissiveInput.Name + "Constant.x);");
                }
            }
            else
            {
                sb.AppendLine("    vec3 emissive = vec3(0.0);");
            }
            sb.AppendLine();
        }

        private void AddLights(StringBuilder sb)
        {
            sb.AppendLine("    vec3 lightPositions[4];");
            sb.AppendLine("    vec3 lightColors[4];");
            sb.AppendLine("");
            sb.AppendLine("    lightPositions[0] = " + TangentLightPos0InputName + ";");
            sb.AppendLine("    lightColors[0] = vec3(30000.0, 30000.0, 30000.0);");
            //sb.AppendLine("    lightPositions[1] = vec3(-10, -10, 10);");
            //sb.AppendLine("    lightColors[1] = vec3(300.0, 0.0, 300.0);");
            sb.AppendLine();
        }

        private void AddNormalCode(StringBuilder sb)
        {
            var normalInput = GetInputForType(ConstructedShaderInputType.Normal);
            if (normalInput == null)
            {
                sb.AppendLine("    vec3 N = normalize(" + NormalInputName + ");");
                sb.AppendLine("    N = vec3(0, 0, 1);");
            }
            else
            {
                if (normalInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    sb.AppendLine("    vec3 N = texture(sampler2D(" + normalInput.Name + "Texture, " + normalInput.Name + "Sampler), " + TexCoordsInputName + ").xyz;");
                    sb.AppendLine("    N = N * 2.0 - 1.0;");
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        internal void ApplyToMaterial(Material material)
        {
            var normalInput = GetInputForType(ConstructedShaderInputType.Normal);
            if (normalInput != null)
            {
                if (normalInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    material.NormalTexture = ((Surface2D)normalInput.Value).Texture;
                }
                else if (normalInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                {
                    throw new NotImplementedException();
                }
            }

            var albedoInput = GetInputForType(ConstructedShaderInputType.DiffuseAlbedo);
            if (albedoInput != null)
            {
                if (albedoInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    material.AlbedoTexture = ((Surface2D)albedoInput.Value).Texture;
                }
                else if (albedoInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                {
                    float[] values = (float[])albedoInput.Value;
                    if (values.Length == 4)
                    {
                        material.AlbedoFactor = new RgbaFloat(values[0], values[1], values[2], values[3]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            var metallicRoughnessInput = GetInputForType(ConstructedShaderInputType.MetallicRoughnessCombined);
            if (metallicRoughnessInput != null)
            {
                if (metallicRoughnessInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    material.MetallicRoughnessTexture = ((Surface2D)metallicRoughnessInput.Value).Texture;
                }
                else if (metallicRoughnessInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                {
                    throw new NotImplementedException();
                }
            }

            var metallicInput = GetInputForType(ConstructedShaderInputType.Metallic);
            if (metallicInput != null)
            {
                if (metallicInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    throw new NotImplementedException(); 
                }
                else if (metallicInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                {
                    material.MetallicFactor = (float)metallicInput.Value;
                }
            }

            var roughnessInput = GetInputForType(ConstructedShaderInputType.Metallic);
            if (roughnessInput != null)
            {
                if (roughnessInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    throw new NotImplementedException();
                }
                else if (roughnessInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                {
                    material.RoughnessFactor = (float)roughnessInput.Value;
                }
            }

            var ambientOcclusionInput = GetInputForType(ConstructedShaderInputType.AmbientOcclusion);
            if (ambientOcclusionInput != null)
            {
                if (ambientOcclusionInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    material.AmbientOcclusionTexture = ((Surface2D)ambientOcclusionInput.Value).Texture;
                }
                else if (ambientOcclusionInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                {
                    material.AmbientOcclusionFactor = (float)ambientOcclusionInput.Value;
                }
            }

            var emissiveInput = GetInputForType(ConstructedShaderInputType.Emissive);
            if (emissiveInput != null)
            {
                if (emissiveInput.ValueType == ConstructedShaderInputValueType.Texture)
                {
                    material.EmissiveTexture = ((Surface2D)emissiveInput.Value).Texture;
                }
                else if (emissiveInput.ValueType == ConstructedShaderInputValueType.ConstantValue)
                {
                    float[] values = (float[])emissiveInput.Value;
                    if (values.Length == 3)
                    {
                        material.EmissiveFactor = new Vector3(values[0], values[1], values[2]);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
