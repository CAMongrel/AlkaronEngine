using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using AlkaronEngine.Graphics;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Util;
using Veldrid;

namespace AlkaronEngine.Assets.Materials
{
    public class Material : Asset, IMaterial
    {
        protected override int MaxAssetVersion => 1;

        public bool RequiresOrderingBackToFront { get; set; }

        private Pipeline graphicsPipeline;
        private ResourceLayout graphicsLayout;
        private DeviceBuffer worldMatrixBuffer;
        private ResourceSet graphicsResourceSet;
        private DeviceBuffer cameraPositionBuffer;
        private TextureView albedoTextureView;
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

        public Shader FragmentShader { get; set; }

        public Material()
        {
            RequiresOrderingBackToFront = false;

            Shader[] shaders = new Shader[]
            {
                AlkaronCoreGame.Core.ShaderManager.StaticMeshVertexShader,
                AlkaronCoreGame.Core.ShaderManager.GetFragmentShaderByName("DefaultPBRFragment")
            };

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    TangentVertex.VertexLayout
                },
                shaders);

            var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

            graphicsLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WorldViewProj", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("CameraBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("AlbedoTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("MetallicRoughnessTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("AlbedoSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("MetallicRoughnessSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                    )
                );

            GraphicsPipelineDescription fullScreenQuadDesc = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, false),
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { graphicsLayout },
                AlkaronCoreGame.Core.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription);

            graphicsPipeline = factory.CreateGraphicsPipeline(ref fullScreenQuadDesc);

            worldMatrixBuffer = factory.CreateBuffer(new BufferDescription(4 * 4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            cameraPositionBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            albedoTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;
            metallicRoughnessTextureView = Graphics2D.ScreenQuad.SingleWhiteTextureView;

            CreateGraphicsResourceSet(factory);
        }

        private void CreateGraphicsResourceSet(ResourceFactory factory)
        {
            graphicsResourceSet?.Dispose();

            graphicsResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                graphicsLayout,
                worldMatrixBuffer,
                cameraPositionBuffer,
                albedoTextureView,
                metallicRoughnessTextureView,
                AlkaronCoreGame.Core.GraphicsDevice.LinearSampler,
                AlkaronCoreGame.Core.GraphicsDevice.LinearSampler));
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

        public void ApplyParameters(RenderContext renderContext, Matrix4x4 worldViewProjection)
        {
            renderContext.CommandList.UpdateBuffer(cameraPositionBuffer, 0, new Vector4(renderContext.RenderManager.ViewTarget.CameraLocation, 1.0f));
            renderContext.CommandList.UpdateBuffer(worldMatrixBuffer, 0, worldViewProjection);
            renderContext.CommandList.SetGraphicsResourceSet(0, graphicsResourceSet);
        }

        public void SetupMaterialForRenderPass(RenderContext renderContext, RenderPass renderPass)
        {
            renderContext.CommandList.SetPipeline(graphicsPipeline);
        }
    }
}
