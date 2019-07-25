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
        private TextureView aoTextureView;
        private TextureView metallicRoughnessTextureView;

        private Texture albedoTexture;
        public Texture AlbedoTexture
        {
            get { return albedoTexture; }
            set
            {
                var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

                albedoTexture = value;
                albedoTextureView = factory.CreateTextureView(albedoTexture);

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

        private Texture aoTexture;
        public Texture AmbientOcclusionTexture
        {
            get { return aoTexture; }
            set
            {
                var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

                aoTexture = value;
                aoTextureView = factory.CreateTextureView(aoTexture);

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

            albedoTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            normalTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            aoTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            metallicRoughnessTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;

            if (ConstructedShader != null)
            {
                graphicsLayout = factory.CreateResourceLayout(
                    ConstructedShader.GetResourceLayoutDescription());

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
                BlendStateDescription.SingleAlphaBlend,
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
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.DiffuseAlbedo))
                {
                    bindableResources.Add(albedoTextureView);
                    bindableResources.Add(AlkaronCoreGame.Core.GraphicsDevice.LinearSampler);
                }
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.MetallicRoughnessCombined))
                {
                    bindableResources.Add(metallicRoughnessTextureView);
                    bindableResources.Add(AlkaronCoreGame.Core.GraphicsDevice.LinearSampler);
                }
                if (ConstructedShader.HasInputOfType(ConstructedShaderInputType.Normal))
                {
                    bindableResources.Add(normalTextureView);
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
            Matrix4x4 worldViewProjection = worldMatrix * renderContext.RenderManager.ViewTarget.ViewMatrix * 
                renderContext.RenderManager.ViewTarget.ProjectionMatrix;

            EnvironmentBuffer environmentBufferObj = new EnvironmentBuffer();
            environmentBufferObj.ViewPosition = new Vector4(renderContext.RenderManager.ViewTarget.CameraLocation, 1.0f);
            environmentBufferObj.LightPosition0 = new Vector4(50.0f, 50.0f, 50.0f, 1.0f);
            environmentBufferObj.LightPosition1 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            environmentBufferObj.LightPosition2 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            environmentBufferObj.LightPosition3 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            renderContext.CommandList.UpdateBuffer(environmentBuffer, 0, environmentBufferObj);
            renderContext.CommandList.UpdateBuffer(worldViewProjMatrixBuffer, 0, worldViewProjection);
            renderContext.CommandList.UpdateBuffer(worldMatrixBuffer, 0, worldMatrix);
            renderContext.CommandList.SetGraphicsResourceSet(0, graphicsResourceSet);
        }

        public void SetupMaterialForRenderPass(RenderContext renderContext, RenderPass renderPass)
        {
            renderContext.CommandList.SetPipeline(graphicsPipeline);
        }
    }
}
