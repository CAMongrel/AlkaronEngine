using AlkaronEngine.Assets.TextureFonts;
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
        private static DeviceBuffer worldMatrixBuffer;
        private static DeviceBuffer colorTintBuffer;
        private static Pipeline graphicsPipeline;
        private static ResourceLayout graphicsLayout;
        private static ResourceSet graphicsResourceSet;

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

            AlkaronCoreGame.Core.GraphicsDevice.UpdateBuffer(worldMatrixBuffer, 0, Matrix4x4.Identity);
            AlkaronCoreGame.Core.GraphicsDevice.UpdateBuffer(colorTintBuffer, 0, RgbaFloat.White);
        }

        private static Vector2 ConvertScreenPosToDevicePos(float x, float y)
        {
            float width = AlkaronCoreGame.Core.Window.Width;
            float height = AlkaronCoreGame.Core.Window.Height;

            Vector2 res;
            res.X = (x / width);
            res.Y = (y / height);

            return res;
        }

        private static Vector2 ConvertScreenScaleToDeviceScale(float x, float y)
        {
            float width = AlkaronCoreGame.Core.Window.Width;
            float height = AlkaronCoreGame.Core.Window.Height;

            Vector2 res;
            res.X = (x / width);
            res.Y = (y / height);

            return res;
        }

        internal static void RenderText(RenderContext renderContext, string text, float x, float y, RgbaFloat color, TextureFont font)
        {
            ScreenQuadVertex[] vertices = new ScreenQuadVertex[text.Length * 6];

            float xPos = x;
            for (int i = 0; i < text.Length; i++)
            {
                var def = font.CharacterDefinitionForGlyph(text[i]);
                font.TexCoordsForCharacterDefinition(def, out Vector2 texCoordStart, out Vector2 texCoordSize);

                Vector2 screenSpacePosition = ConvertScreenPosToDevicePos(xPos - def.OriginX, y - def.OriginY + (font.FontDefinition.Size / 2));
                Vector2 screenSpaceSize = ConvertScreenScaleToDeviceScale(def.Width, def.Height);

                xPos += def.Advance;                

                vertices[i * 6 + 0] = new ScreenQuadVertex()
                {
                    Position = screenSpacePosition,
                    TexCoord = texCoordStart
                };
                vertices[i * 6 + 1] = new ScreenQuadVertex()
                {
                    Position = screenSpacePosition + new Vector2(screenSpaceSize.X, 0),
                    TexCoord = texCoordStart + new Vector2(texCoordSize.X, 0)
                };
                vertices[i * 6 + 2] = new ScreenQuadVertex()
                {
                    Position = screenSpacePosition + new Vector2(0, screenSpaceSize.Y),
                    TexCoord = texCoordStart + new Vector2(0, texCoordSize.Y)
                };
                vertices[i * 6 + 3] = new ScreenQuadVertex()
                {
                    Position = screenSpacePosition + new Vector2(screenSpaceSize.X, 0),
                    TexCoord = texCoordStart + new Vector2(texCoordSize.X, 0)
                };
                vertices[i * 6 + 4] = new ScreenQuadVertex()
                {
                    Position = screenSpacePosition + screenSpaceSize,
                    TexCoord = texCoordStart + texCoordSize
                };
                vertices[i * 6 + 5] = new ScreenQuadVertex()
                {
                    Position = screenSpacePosition + new Vector2(0, screenSpaceSize.Y),
                    TexCoord = texCoordStart + new Vector2(0, texCoordSize.Y)
                };
            }

            ResourceFactory factory = renderContext.GraphicsDevice.ResourceFactory;
            graphicsResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                graphicsLayout,
                worldMatrixBuffer,
                font.Surface.View,
                AlkaronCoreGame.Core.GraphicsDevice.PointSampler,
                colorTintBuffer));

            renderContext.CommandList.SetGraphicsResourceSet(0, graphicsResourceSet);

            DeviceBuffer vertexBuffer = factory.CreateBuffer(
                new BufferDescription((uint)(vertices.Length * 4 * sizeof(float)), BufferUsage.VertexBuffer));
            AlkaronCoreGame.Core.GraphicsDevice.UpdateBuffer<ScreenQuadVertex>(vertexBuffer, 0, vertices);

            renderContext.CommandList.UpdateBuffer(worldMatrixBuffer, 0, Matrix4x4.Identity);
            renderContext.CommandList.UpdateBuffer(colorTintBuffer, 0, color);

            renderContext.CommandList.SetPipeline(graphicsPipeline);
            renderContext.CommandList.SetVertexBuffer(0, vertexBuffer);

            renderContext.CommandList.Draw((uint)vertices.Length);
        }
    }
}
