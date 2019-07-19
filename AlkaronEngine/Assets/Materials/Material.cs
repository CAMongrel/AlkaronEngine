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

        public Shader FragmentShader { get; set; }

        public Material()
        {
            RequiresOrderingBackToFront = false;

            Shader[] shaders = new Shader[]
            {
                AlkaronCoreGame.Core.ShaderManager.StaticMeshVertexShader,
                AlkaronCoreGame.Core.ShaderManager.GetFragmentShaderByName("SimpleColorFragment")
            };

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    TangentVertex.VertexLayout
                },
                shaders);

            var factory = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory;

            graphicsLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WorldViewProj", ResourceKind.UniformBuffer, ShaderStages.Vertex))
                /*new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ColorTint", ResourceKind.UniformBuffer, ShaderStages.Fragment))*/);

            GraphicsPipelineDescription fullScreenQuadDesc = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, false),
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { graphicsLayout },
                AlkaronCoreGame.Core.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription);

            graphicsPipeline = factory.CreateGraphicsPipeline(ref fullScreenQuadDesc);

            worldMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            graphicsResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                graphicsLayout,
                worldMatrixBuffer));
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

        public void ApplyParameters(RenderContext renderContext, Matrix4x4 worldViewProjectio)
        {
            renderContext.CommandList.UpdateBuffer(worldMatrixBuffer, 0, worldViewProjectio);
            renderContext.CommandList.SetGraphicsResourceSet(0, graphicsResourceSet);
        }

        public void SetupMaterialForRenderPass(RenderContext renderContext, RenderPass renderPass)
        {
            renderContext.CommandList.SetPipeline(graphicsPipeline);
        }
    }
}
