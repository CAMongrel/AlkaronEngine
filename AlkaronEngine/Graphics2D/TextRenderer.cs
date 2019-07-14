using AlkaronEngine.Graphics3D;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace AlkaronEngine.Graphics2D
{
    internal static class TextRenderer
    {
        private static DeviceBuffer vertexBuffer;
        private static DeviceBuffer indexBuffer;
        private static DeviceBuffer worldMatrixBuffer;
        private static DeviceBuffer colorTintBuffer;
        private static Pipeline graphicsPipeline;
        private static ResourceLayout graphicsLayout;
        private static ResourceSet graphicsResourceSet;
        private static TextureView targetTextureView;

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

        internal static void RenderText(RenderContext renderContext, string text, float x, float y, RgbaFloat color)
        {

        }
    }
}
