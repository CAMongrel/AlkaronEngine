using AlkaronEngine.Graphics3D;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.SPIRV;

namespace AlkaronEngine.Graphics2D
{
    internal struct ScreenQuadVertex
    {
        public Vector2 Position;
        public Vector2 TexCoord;
    }

    internal class ScreenQuad
    {
        private static DeviceBuffer vertexBuffer;
        private static DeviceBuffer indexBuffer;
        private static Pipeline graphicsPipeline;
        private static ResourceLayout graphicsLayout;
        private static ResourceSet graphicsResourceSet;
        private static Texture targetTexture;
        private static TextureView targetTextureView;

        internal static void Initialize(ResourceFactory factory)
        {
            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(
                    ShaderStages.Vertex,
                    AlkaronCoreGame.Core.AlkaronContent.OpenResourceBytes("Vertex.glsl").ToArray(),
                    "main"),
                new ShaderDescription(
                    ShaderStages.Fragment,
                    AlkaronCoreGame.Core.AlkaronContent.OpenResourceBytes("Fragment.glsl").ToArray(),
                    "main"));

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new VertexLayoutDescription[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
                },
                shaders);

            vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * sizeof(float), BufferUsage.VertexBuffer));
            indexBuffer = factory.CreateBuffer(new BufferDescription(6 * sizeof(ushort), BufferUsage.IndexBuffer));

            graphicsLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            GraphicsPipelineDescription fullScreenQuadDesc = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { graphicsLayout },
                AlkaronCoreGame.Core.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription);

            graphicsPipeline = factory.CreateGraphicsPipeline(ref fullScreenQuadDesc);

            ScreenQuadVertex[] vertices = new ScreenQuadVertex[4];
            vertices[0] = new ScreenQuadVertex { Position = new Vector2(-1, 1), TexCoord = new Vector2(0, 0) };
            vertices[1] = new ScreenQuadVertex { Position = new Vector2(1, 1), TexCoord = new Vector2(1, 0) };
            vertices[2] = new ScreenQuadVertex { Position = new Vector2(-1, -1), TexCoord = new Vector2(0, 1) };
            vertices[3] = new ScreenQuadVertex { Position = new Vector2(1, -1), TexCoord = new Vector2(1, 1) };

            ushort[] indices = { 0, 1, 2, 1, 3, 2 };

            AlkaronCoreGame.Core.GraphicsDevice.UpdateBuffer<ScreenQuadVertex>(vertexBuffer, 0, vertices);
            AlkaronCoreGame.Core.GraphicsDevice.UpdateBuffer<ushort>(indexBuffer, 0, indices);

            targetTexture = factory.CreateTexture(TextureDescription.Texture2D(
                128,
                128,
                1,
                1,
                PixelFormat.R32_G32_B32_A32_Float,
                TextureUsage.Sampled | TextureUsage.Storage));

            targetTextureView = factory.CreateTextureView(targetTexture);

            graphicsResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                graphicsLayout,
                targetTextureView,
                AlkaronCoreGame.Core.GraphicsDevice.PointSampler));
        }

        internal static void RenderConfigDidUpdate()
        {
            /*if (quadEffect == null)
            {
                return;
            }

            if (vbuffer.GraphicsDevice != renderConfig.GraphicsDevice)
            {
                throw new NotImplementedException("Different GraphicsDevices at runtime are not yet supported");
            }
            if (quadEffect.GraphicsDevice != renderConfig.GraphicsDevice)
            {
                throw new NotImplementedException("Different GraphicsDevices at runtime are not yet supported");
            }

            quadEffect.Projection = Matrix.CreateOrthographicOffCenter(0, renderConfig.ScreenSize.X, renderConfig.ScreenSize.Y, 0, 0, -500);*/
        }

        internal static void RenderQuad(RenderContext renderContext /*Vector2 screenPosition, Vector2 size, Color col,
                                        float rotation, Texture2D texture, IRenderConfiguration currentRenderConfig*/)
        {
            renderContext.CommandList.SetPipeline(graphicsPipeline);
            renderContext.CommandList.SetVertexBuffer(0, vertexBuffer);
            renderContext.CommandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            renderContext.CommandList.SetGraphicsResourceSet(0, graphicsResourceSet);
            renderContext.CommandList.DrawIndexed(6, 1, 0, 0, 0);

            /*if (renderConfig == null)
            {
                throw new InvalidOperationException(nameof(renderConfig) + " must not be null (call Initialize first)");
            }

            if (currentRenderConfig != renderConfig)
            {
                renderConfig = currentRenderConfig;
                RenderConfigDidUpdate();
            }

            renderConfig.GraphicsDevice.SetVertexBuffer(vbuffer);

            Matrix worldMat = Matrix.Identity;

            if (rotation > 0)
            {
                worldMat = Matrix.CreateScale(size.X, size.Y, 0) *
                   Matrix.CreateTranslation(-size.X * 0.5f, -size.Y * 0.5f, 0) *
                   Matrix.CreateRotationZ(rotation) *
                   Matrix.CreateTranslation(size.X * 0.5f, size.Y * 0.5f, 0) *
                   Matrix.CreateTranslation(screenPosition.X, screenPosition.Y, 0);
            }
            else
            {
                worldMat = Matrix.CreateScale(size.X, size.Y, 0) *
                   Matrix.CreateTranslation(screenPosition.X, screenPosition.Y, 0);
            }

            quadEffect.World = worldMat;
            quadEffect.Texture = texture;
            quadEffect.DiffuseColor = col.ToVector3();
            quadEffect.Alpha = (float)col.A / 255.0f;

            renderConfig.GraphicsDevice.BlendState = blendState;

            renderConfig.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;

            for (int i = 0; i < quadEffect.CurrentTechnique.Passes.Count; i++)
            {
                quadEffect.CurrentTechnique.Passes[i].Apply();

                renderConfig.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }*/
        }
    }
}
