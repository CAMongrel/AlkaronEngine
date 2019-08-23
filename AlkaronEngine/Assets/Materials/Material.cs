using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using AlkaronEngine.Graphics;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Util;
using Veldrid;

namespace AlkaronEngine.Assets.Materials
{
    [StructLayout(LayoutKind.Explicit)]
    struct EnvironmentBuffer
    {
        [FieldOffset(0)]
        public Vector4 ViewPosition;
        [FieldOffset(16)]
        public Vector4 LightPosition0;
        [FieldOffset(32)]
        public Vector4 LightPosition1;
        [FieldOffset(48)]
        public Vector4 LightPosition2;
        [FieldOffset(64)]
        public Vector4 LightPosition3;
    }

    public class Material : Asset, IMaterial
    {
        internal static readonly string DefaultPBRFragmentName = "DefaultPBRFragment";

        protected override int MaxAssetVersion => 1;

        public bool RequiresOrderingBackToFront { get; set; }

        private Pipeline graphicsPipeline;
        private ResourceLayout graphicsLayout;
        private DeviceBuffer worldMatrixBuffer;
        private DeviceBuffer worldViewProjMatrixBuffer;
        private DeviceBuffer environmentBuffer;
        private DeviceBuffer jointMatricesBuffer;
        private ResourceSet graphicsResourceSet;

        private TextureView albedoTextureView;
        private TextureView normalTextureView;
        private TextureView ambientOcclusionTextureView;
        private TextureView metallicRoughnessTextureView;
        private TextureView emissiveTextureView;

        private DeviceBuffer albedoFactorBuffer;
        private DeviceBuffer metallicFactorBuffer;
        private DeviceBuffer roughnessFactorBuffer;
        private DeviceBuffer ambientOcclusionFactorBuffer;
        private DeviceBuffer emissiveFactorBuffer;

        public RgbaFloat AlbedoFactor = RgbaFloat.Black;
        public float MetallicFactor = 1.0f;
        public float RoughnessFactor = 1.0f;
        public float AmbientOcclusionFactor = 1.0f;
        public Vector3 EmissiveFactor = Vector3.Zero;

        private Texture albedoTexture;
        public Texture AlbedoTexture
        {
            get { return albedoTexture; }
            set
            {
                var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

                albedoTexture = value;
                try
                {
                    albedoTextureView = factory.CreateTextureView(albedoTexture);
                }
                catch
                {
                    albedoTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
                }                

                CreateGraphicsResourceSet(factory);
            }
        }

        private Texture normalTexture;
        public Texture NormalTexture
        {
            get { return normalTexture; }
            set
            {
                var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

                normalTexture = value;
                normalTextureView = factory.CreateTextureView(normalTexture);

                CreateGraphicsResourceSet(factory);
            }
        }

        private Texture ambientOcclusionTexture;
        public Texture AmbientOcclusionTexture
        {
            get { return ambientOcclusionTexture; }
            set
            {
                var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

                ambientOcclusionTexture = value;
                ambientOcclusionTextureView = factory.CreateTextureView(ambientOcclusionTexture);

                CreateGraphicsResourceSet(factory);
            }
        }

        private Texture metallicRoughnessTexture;
        public Texture MetallicRoughnessTexture
        {
            get { return metallicRoughnessTexture; }
            set
            {
                var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

                metallicRoughnessTexture = value;
                metallicRoughnessTextureView = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory.CreateTextureView(metallicRoughnessTexture);

                CreateGraphicsResourceSet(factory);
            }
        }

        private Texture emissiveTexture;
        public Texture EmissiveTexture
        {
            get { return emissiveTexture; }
            set
            {
                var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

                emissiveTexture = value;
                emissiveTextureView = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory.CreateTextureView(emissiveTexture);

                CreateGraphicsResourceSet(factory);
            }
        }

        internal Shader FragmentShader { get; set; }

        internal ConstructedShader? ConstructedShader { get; private set; }

        public Material()
        {
            RequiresOrderingBackToFront = false;
        }

        internal void LoadFromConstructedShader(ConstructedShader constructedShader)
        {
            ConstructedShader = constructedShader;

            string code = constructedShader.GenerateShaderCode();
            AlkaronCoreGame.Core.ShaderManager.CompileFragmentShader(constructedShader.Name, code, constructedShader.IsSkeletalShader);

            LoadFragmentShader(constructedShader);
        }

        internal void LoadFragmentShader(ConstructedShader constructedShader)
        {
            Shader[] shaders = new Shader[]
            {
                constructedShader.VertexShader,
                AlkaronCoreGame.Core.ShaderManager.GetFragmentShaderByName(constructedShader.Name)
            };

            VertexLayoutDescription usedVDesc = TangentVertex.VertexLayout;
            if (constructedShader.IsSkeletalShader)
            {
                usedVDesc = SkinnedTangentVertex.VertexLayout;
            }

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    usedVDesc
                },
                shaders);

            var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

            worldMatrixBuffer = factory.CreateBuffer(new BufferDescription(4 * 4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            worldViewProjMatrixBuffer = factory.CreateBuffer(new BufferDescription(4 * 4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            environmentBuffer = factory.CreateBuffer(new BufferDescription(5 * 4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            if (constructedShader.IsSkeletalShader)
            {
                jointMatricesBuffer = factory.CreateBuffer(new BufferDescription(80 * 4 * 4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            }

            // TODO: Adapt if optimized packing is used in shader
            albedoFactorBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            metallicFactorBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            roughnessFactorBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            emissiveFactorBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            ambientOcclusionFactorBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            albedoTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            normalTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            ambientOcclusionTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            metallicRoughnessTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            emissiveTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;

            BlendStateDescription blendStateDesc = BlendStateDescription.SingleAlphaBlend;
            if (ConstructedShader != null)
            {
                graphicsLayout = factory.CreateResourceLayout(
                    ConstructedShader.GetResourceLayoutDescription());

                switch (ConstructedShader.BlendMode)
                {
                    case BlendMode.Opaque:
                        blendStateDesc = BlendStateDescription.SingleDisabled;
                        RequiresOrderingBackToFront = false;
                        break;
                    case BlendMode.Mask:
                        blendStateDesc = BlendStateDescription.SingleOverrideBlend;
                        RequiresOrderingBackToFront = false;
                        break;
                    case BlendMode.Blend:
                        blendStateDesc = BlendStateDescription.SingleAlphaBlend;
                        RequiresOrderingBackToFront = true;
                        break;
                }

                ConstructedShader.ApplyToMaterial(this);
            }
            else
            {
                graphicsLayout = factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("WorldViewProj", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                        new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                        new ResourceLayoutElementDescription("CameraBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("AlbedoTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("AlbedoSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("MetallicRoughnessTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("MetallicRoughnessSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                        )
                    );
            }

            GraphicsPipelineDescription fullScreenQuadDesc = new GraphicsPipelineDescription(
                blendStateDesc,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, false),
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { graphicsLayout },
                AlkaronCoreGame.Core.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription);

            graphicsPipeline = factory.CreateGraphicsPipeline(ref fullScreenQuadDesc);

            CreateGraphicsResourceSet(factory);
        }

        private void CreateGraphicsResourceSet(ResourceFactory factory)
        {
            graphicsResourceSet?.Dispose();

            List<BindableResource> bindableResources = new List<BindableResource>();
            bindableResources.Add(worldViewProjMatrixBuffer);
            bindableResources.Add(worldMatrixBuffer);
            bindableResources.Add(environmentBuffer);
            if (ConstructedShader != null)
            {
                if (ConstructedShader.IsSkeletalShader)
                {
                    bindableResources.Add(jointMatricesBuffer);
                }

                // DiffuseAlbedo
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.DiffuseAlbedo, ConstructedShaderInputValueType.Texture))
                {
                    bindableResources.Add(albedoTextureView);
                    bindableResources.Add(AlkaronCoreGame.Core.GraphicsDevice.Aniso4xSampler);
                }
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.DiffuseAlbedo, ConstructedShaderInputValueType.ConstantValue))
                {
                    bindableResources.Add(albedoFactorBuffer);
                }

                // Metallic/Roughness
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.MetallicRoughnessCombined, ConstructedShaderInputValueType.Texture))
                {
                    bindableResources.Add(metallicRoughnessTextureView);
                    bindableResources.Add(AlkaronCoreGame.Core.GraphicsDevice.LinearSampler);
                }
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.Metallic, ConstructedShaderInputValueType.ConstantValue))
                {
                    bindableResources.Add(metallicFactorBuffer);
                }
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.Roughness, ConstructedShaderInputValueType.ConstantValue))
                {
                    bindableResources.Add(roughnessFactorBuffer);
                }

                // Normal
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.Normal, ConstructedShaderInputValueType.Texture))
                {
                    bindableResources.Add(normalTextureView);
                    bindableResources.Add(AlkaronCoreGame.Core.GraphicsDevice.LinearSampler);
                }

                // AmbientOcclusion
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.AmbientOcclusion, ConstructedShaderInputValueType.Texture))
                {
                    bindableResources.Add(ambientOcclusionTextureView);
                    bindableResources.Add(AlkaronCoreGame.Core.GraphicsDevice.LinearSampler);
                }
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.AmbientOcclusion, ConstructedShaderInputValueType.ConstantValue))
                {
                    bindableResources.Add(ambientOcclusionFactorBuffer);
                }

                // Emissive
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.Emissive, ConstructedShaderInputValueType.Texture))
                {
                    bindableResources.Add(emissiveTextureView);
                    bindableResources.Add(AlkaronCoreGame.Core.GraphicsDevice.LinearSampler);
                }
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.Emissive, ConstructedShaderInputValueType.ConstantValue))
                {
                    bindableResources.Add(emissiveFactorBuffer);
                }
            }
            else
            {
                bindableResources.Add(albedoTextureView);
                bindableResources.Add(AlkaronCoreGame.Core.GraphicsDevice.LinearSampler);
                bindableResources.Add(metallicRoughnessTextureView);
                bindableResources.Add(AlkaronCoreGame.Core.GraphicsDevice.LinearSampler);
            }

            graphicsResourceSet = factory.CreateResourceSet(
                new ResourceSetDescription(
                    graphicsLayout,
                    bindableResources.ToArray()
                ));
        }

        public override bool IsValid => false;

        private void CreateShader()
        {
        }

        internal override void Deserialize(BinaryReader reader, AssetSettings assetSettings)
        {
            base.Deserialize(reader, assetSettings);

            RequiresOrderingBackToFront = reader.ReadBoolean();

            CreateShader();
        }

        internal override void Serialize(BinaryWriter writer, AssetSettings assetSettings)
        {
            base.Serialize(writer, assetSettings);

            writer.Write(RequiresOrderingBackToFront);
        }

        public void ApplyParameters(RenderContext renderContext, Matrix4x4 worldMatrix, Matrix4x4[]? boneMatrices = null)
        {
            Performance.StartAppendAggreate("Setup Texture");
            Matrix4x4 worldViewProjection = worldMatrix * renderContext.RenderManager.ViewTarget.ViewMatrix * 
                renderContext.RenderManager.ViewTarget.ProjectionMatrix;

            EnvironmentBuffer environmentBufferObj = new EnvironmentBuffer();
            environmentBufferObj.ViewPosition = new Vector4(renderContext.RenderManager.ViewTarget.CameraLocation, 1.0f);
            environmentBufferObj.LightPosition0 = new Vector4(50.0f, 50.0f, 50.0f, 1.0f);
            environmentBufferObj.LightPosition1 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            environmentBufferObj.LightPosition2 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            environmentBufferObj.LightPosition3 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            // Vertex Shader
            renderContext.CommandList.UpdateBuffer(worldViewProjMatrixBuffer, 0, worldViewProjection);
            renderContext.CommandList.UpdateBuffer(worldMatrixBuffer, 0, worldMatrix);
            renderContext.CommandList.UpdateBuffer(environmentBuffer, 0, environmentBufferObj);
            if (ConstructedShader != null &&
                ConstructedShader.IsSkeletalShader &&
                boneMatrices != null)
            {
                if (boneMatrices.Length > 80)
                {
                    throw new NotSupportedException("boneMatrices can have a maximum length of 80 matrices");
                }

                renderContext.CommandList.UpdateBuffer(jointMatricesBuffer, 0, boneMatrices);
            }

            // Fragment Shader
            renderContext.CommandList.UpdateBuffer(metallicFactorBuffer, 0, MetallicFactor);
            renderContext.CommandList.UpdateBuffer(roughnessFactorBuffer, 0, RoughnessFactor);
            renderContext.CommandList.UpdateBuffer(albedoFactorBuffer, 0, AlbedoFactor);
            renderContext.CommandList.UpdateBuffer(ambientOcclusionFactorBuffer, 0, AmbientOcclusionFactor);
            renderContext.CommandList.UpdateBuffer(emissiveFactorBuffer, 0, EmissiveFactor);

            renderContext.CommandList.SetGraphicsResourceSet(0, graphicsResourceSet);
            Performance.EndAppendAggreate("Setup Texture");
        }

        public void SetupMaterialForRenderPass(RenderContext renderContext)
        {
            Performance.StartAppendAggreate("Setup");
            renderContext.CommandList.SetPipeline(graphicsPipeline);
            Performance.EndAppendAggreate("Setup");
        }
    }
}
