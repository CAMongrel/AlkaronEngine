using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics2D
{
   internal struct ScreenQuadVertex
   {
      public Vector4 Position;
      public Vector2 TexCoord;
   }

   internal class ScreenQuad
   {
      private static ScreenQuadVertex[] vertices;
      private static VertexDeclaration quadDecl;
      private static IRenderConfiguration renderConfig;
      private static VertexBuffer vbuffer;
      private static BasicEffect quadEffect;
      private static BlendState blendState;
      private static SamplerState samplerState;

      internal static void Initialize(IRenderConfiguration setRenderConfig)
      {
         renderConfig = setRenderConfig;

         quadDecl = new VertexDeclaration(new VertexElement[]
         {
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
            new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
         });

         vertices = new ScreenQuadVertex[4];
         vertices[0] = new ScreenQuadVertex { Position = new Vector4(0, 0, 0, 1), TexCoord = new Vector2(0, 0) };
         vertices[1] = new ScreenQuadVertex { Position = new Vector4(1, 0, 0, 1), TexCoord = new Vector2(1, 0) };
         vertices[2] = new ScreenQuadVertex { Position = new Vector4(0, 1, 0, 1), TexCoord = new Vector2(0, 1) };
         vertices[3] = new ScreenQuadVertex { Position = new Vector4(1, 1, 0, 1), TexCoord = new Vector2(1, 1) };

         blendState = BlendState.AlphaBlend;

         vbuffer = new VertexBuffer(renderConfig.GraphicsDevice, quadDecl, 4, BufferUsage.WriteOnly);
         vbuffer.SetData<ScreenQuadVertex>(vertices);

         quadEffect = new BasicEffect(renderConfig.GraphicsDevice);
         quadEffect.FogEnabled = false;
         quadEffect.LightingEnabled = false;
         quadEffect.TextureEnabled = true;
         quadEffect.DiffuseColor = new Vector3(1, 1, 1);

         quadEffect.World = Matrix.Identity;
         quadEffect.Projection = Matrix.CreateOrthographicOffCenter(0, renderConfig.ScreenSize.X, renderConfig.ScreenSize.Y, 0, 0, -500);
         quadEffect.View = Matrix.Identity;
      }

      internal static void RenderConfigDidUpdate()
      {
         if (quadEffect == null)
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

         quadEffect.Projection = Matrix.CreateOrthographicOffCenter(0, renderConfig.ScreenSize.X, renderConfig.ScreenSize.Y, 0, 0, -500);
      }

      internal static void RenderQuad(Vector2 screenPosition, Vector2 size, Color col, 
                                      float rotation, Texture2D texture, IRenderConfiguration currentRenderConfig)
      {
         if (renderConfig == null)
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
         }
      }
   }
}
