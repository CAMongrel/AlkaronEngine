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
        private ResourceSet graphicsResourceSet;

        private TextureView albedoTextureView;
        private TextureView normalTextureView;
        private TextureView ambientOcclusionTextureView;
        private TextureView metallicRoughnessTextureView;

        private DeviceBuffer albedoFactorBuffer;
        private DeviceBuffer metallicFactorBuffer;
        private DeviceBuffer roughnessFactorBuffer;

        public RgbaFloat AlbedoFactor = RgbaFloat.Black;
        public float MetallicFactor = 1.0f;
        public float RoughnessFactor = 1.0f;

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
            AlkaronCoreGame.Core.ShaderManager.CompileFragmentShader(constructedShader.Name, code);

            LoadFragmentShader(constructedShader.Name);
        }

        internal void LoadFragmentShader(string name)
        {
            Shader[] shaders = new Shader[]
            {
                AlkaronCoreGame.Core.ShaderManager.StaticMeshVertexShader,
                AlkaronCoreGame.Core.ShaderManager.GetFragmentShaderByName(name)
            };

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    TangentVertex.VertexLayout
                },
                shaders);

            var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

            worldMatrixBuffer = factory.CreateBuffer(new BufferDescription(4 * 4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            worldViewProjMatrixBuffer = factory.CreateBuffer(new BufferDescription(4 * 4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            environmentBuffer = factory.CreateBuffer(new BufferDescription(5 * 4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            // TODO: Adapt if optimized packing is used in shader
            albedoFactorBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            metallicFactorBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            roughnessFactorBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            albedoTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            normalTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            ambientOcclusionTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            metallicRoughnessTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;

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

        public void ApplyParameters(RenderContext renderContext, Matrix4x4 worldMatrix)
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

            renderContext.CommandList.UpdateBuffer(metallicFactorBuffer, 0, MetallicFactor);
            renderContext.CommandList.UpdateBuffer(roughnessFactorBuffer, 0, RoughnessFactor);
            renderContext.CommandList.UpdateBuffer(albedoFactorBuffer, 0, AlbedoFactor);
            renderContext.CommandList.UpdateBuffer(environmentBuffer, 0, environmentBufferObj);
            renderContext.CommandList.UpdateBuffer(worldViewProjMatrixBuffer, 0, worldViewProjection);
            renderContext.CommandList.UpdateBuffer(worldMatrixBuffer, 0, worldMatrix);
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
