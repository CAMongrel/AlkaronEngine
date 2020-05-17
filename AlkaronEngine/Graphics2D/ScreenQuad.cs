using AlkaronEngine.Graphics3D;
using AlkaronEngine.Util;
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

    internal static class ScreenQuad
    {
        private static DeviceBuffer vertexBuffer;
        private static DeviceBuffer indexBuffer;
        private static DeviceBuffer worldMatrixBuffer;
        private static DeviceBuffer colorTintBuffer;
        private static Pipeline graphicsPipeline;
        private static ResourceLayout graphicsLayout;
        private static ResourceSet graphicsResourceSet;
        private static TextureView targetTextureView;

        internal static Texture SingleWhiteTexture { get; private set; }
        internal static TextureView SingleWhiteTextureView { get; private set; }

        internal static void Initialize(ResourceFactory factory)
        {
            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(
                    ShaderStages.Vertex,
                    AlkaronCoreGame.Core.AlkaronContent.OpenResourceBytes("ScreenQuadVertex.glsl").ToArray(),
                    "main"),
                new ShaderDescription(
                    ShaderStages.Fragment,
                    AlkaronCoreGame.Core.AlkaronContent.OpenResourceBytes("ScreenQuadFragment.glsl").ToArray(),
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
            worldMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            colorTintBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(float), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            graphicsLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ColorTint", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

            GraphicsPipelineDescription fullScreenQuadDesc = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { graphicsLayout },
                AlkaronCoreGame.Core.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription);

            graphicsPipeline = factory.CreateGraphicsPipeline(ref fullScreenQuadDesc);

            ScreenQuadVertex[] vertices = new ScreenQuadVertex[4];
            vertices[0] = new ScreenQuadVertex { Position = new Vector2(0, 0), TexCoord = new Vector2(0, 0) };
            vertices[1] = new ScreenQuadVertex { Position = new Vector2(1, 0), TexCoord = new Vector2(1, 0) };
            vertices[2] = new ScreenQuadVertex { Position = new Vector2(0, 1), TexCoord = new Vector2(0, 1) };
            vertices[3] = new ScreenQuadVertex { Position = new Vector2(1, 1), TexCoord = new Vector2(1, 1) };

            ushort[] indices = { 0, 1, 2, 1, 3, 2 };

            AlkaronCoreGame.Core.GraphicsDevice.UpdateBuffer<ScreenQuadVertex>(vertexBuffer, 0, vertices);
            AlkaronCoreGame.Core.GraphicsDevice.UpdateBuffer<ushort>(indexBuffer, 0, indices);
            AlkaronCoreGame.Core.GraphicsDevice.UpdateBuffer(worldMatrixBuffer, 0, Matrix4x4.Identity);
            AlkaronCoreGame.Core.GraphicsDevice.UpdateBuffer(colorTintBuffer, 0, RgbaFloat.White);

            SingleWhiteTexture = factory.CreateTexture(new TextureDescription()
            {
                Width = 1,
                Height = 1,
                Depth = 1,
                Format = PixelFormat.R8_G8_B8_A8_UNorm,
                Usage = TextureUsage.Sampled,
                ArrayLayers = 1,
                MipLevels = 1,
                SampleCount = TextureSampleCount.Count1,
                Type = TextureType.Texture2D
            });
            AlkaronCoreGame.Core.GraphicsDevice.UpdateTexture(SingleWhiteTexture, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, 0, 0, 1, 1, 1, 0, 0);

            SingleWhiteTextureView = factory.CreateTextureView(SingleWhiteTexture);
        }

        private static Vector2 ConvertScreenPosToDevicePos(Vector2 vec)
        {
            float width = AlkaronCoreGame.Core.Window.Width;
            float height = AlkaronCoreGame.Core.Window.Height;

            Vector2 res;
            res.X = (vec.X / width);
            res.Y = (vec.Y / height);

            return res;
        }

        private static Vector2 ConvertScreenScaleToDeviceScale(Vector2 vec)
        {
            float width = AlkaronCoreGame.Core.Window.Width;
            float height = AlkaronCoreGame.Core.Window.Height;

            Vector2 res;
            res.X = (vec.X / width);
            res.Y = (vec.Y / height);

            return res;
        }

        internal static void RenderRectangle(RenderContext renderContext, RectangleF destRect, RgbaFloat col, int BorderWidth = 1)
        {
            Vector2 Scale = Vector2.One;
            Vector2 ScaledOffset = Vector2.Zero;

            destRect = new RectangleF(ScaledOffset.X + (int)(destRect.X * Scale.X), ScaledOffset.Y + (int)(destRect.Y * Scale.Y),
               (int)(destRect.Width * Scale.X), (int)(destRect.Height * Scale.Y));

            Rectangle singleRect = new Rectangle(0, 0, 1, 1);
            Rectangle leftRect = new Rectangle((int)destRect.X, (int)destRect.Y, BorderWidth, (int)destRect.Height);
            Rectangle topRect = new Rectangle((int)destRect.X, (int)destRect.Y, (int)destRect.Width, BorderWidth);
            Rectangle rightRect = new Rectangle((int)destRect.X + (int)destRect.Width - BorderWidth, (int)destRect.Y, BorderWidth, (int)destRect.Height);
            Rectangle bottomRect = new Rectangle((int)destRect.X, (int)destRect.Y + (int)destRect.Height - BorderWidth, (int)destRect.Width, BorderWidth);

            RenderQuad(renderContext, new Vector2(leftRect.X, leftRect.Y), new Vector2(leftRect.Width, leftRect.Height),
                SingleWhiteTexture, col);
            RenderQuad(renderContext, new Vector2(topRect.X, topRect.Y), new Vector2(topRect.Width, topRect.Height),
                SingleWhiteTexture, col);
            RenderQuad(renderContext, new Vector2(rightRect.X, rightRect.Y), new Vector2(rightRect.Width, rightRect.Height),
                SingleWhiteTexture, col);
            RenderQuad(renderContext, new Vector2(bottomRect.X, bottomRect.Y), new Vector2(bottomRect.Width, bottomRect.Height),
                SingleWhiteTexture, col);
        }

        internal static void FillRectangle(RenderContext renderContext, RectangleF destRect, RgbaFloat col)
        {
            Vector2 Scale = Vector2.One;
            Vector2 ScaledOffset = Vector2.Zero;

            destRect = new RectangleF(ScaledOffset.X + (int)(destRect.X * Scale.X), ScaledOffset.Y + (int)(destRect.Y * Scale.Y),
               (int)(destRect.Width * Scale.X), (int)(destRect.Height * Scale.Y));

            RenderQuad(renderContext, new Vector2(destRect.X, destRect.Y), new Vector2(destRect.Width, destRect.Height),
                SingleWhiteTexture, col);
        }

        internal static void RenderQuad(RenderContext renderContext, Vector2 screenPos, Vector2 size,
                                        Texture texture, RgbaFloat color, float rotation = 0)
        {
            screenPos = ConvertScreenPosToDevicePos(screenPos);
            size = ConvertScreenScaleToDeviceScale(size);

            Matrix4x4 worldMat = Matrix4x4.Identity;
            if (rotation != 0.0f)
            {
                worldMat = Matrix4x4.CreateScale(size.X, size.Y, 0) *
                   Matrix4x4.CreateTranslation(-size.X * 0.5f, -size.Y * 0.5f, 0) *
                   Matrix4x4.CreateRotationZ(rotation) *
                   Matrix4x4.CreateTranslation(size.X * 0.5f, size.Y * 0.5f, 0) *
                   Matrix4x4.CreateTranslation(screenPos.X, screenPos.Y, 0);
            }
            else
            {
                worldMat = Matrix4x4.CreateScale(size.X, size.Y, 0) *
                   Matrix4x4.CreateTranslation(screenPos.X, screenPos.Y, 0);
            }

            renderContext.CommandList.UpdateBuffer(worldMatrixBuffer, 0, worldMat);
            renderContext.CommandList.UpdateBuffer(colorTintBuffer, 0, color);

            renderContext.CommandList.SetPipeline(graphicsPipeline);
            renderContext.CommandList.SetVertexBuffer(0, vertexBuffer);
            renderContext.CommandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);

            ResourceFactory factory = renderContext.GraphicsDevice.ResourceFactory;
            if (texture != null)
            {
                if (texture == SingleWhiteTexture)
                {
                    targetTextureView = SingleWhiteTextureView;
                }
                else
                {
                    targetTextureView = factory.CreateTextureView(texture);
                }

                graphicsResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                    graphicsLayout,
                    worldMatrixBuffer,
                    targetTextureView,                    
                    AlkaronCoreGame.Core.GraphicsDevice.LinearSampler,
                    colorTintBuffer));
            }
            else
            {
                graphicsResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                    graphicsLayout,
                    worldMatrixBuffer,
                    null,
                    null,
                    colorTintBuffer));
            }

            renderContext.CommandList.SetGraphicsResourceSet(0, graphicsResourceSet);

            renderContext.CommandList.DrawIndexed(6, 1, 0, 0, 0);
        }
    }
}
