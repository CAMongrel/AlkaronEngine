using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class ComponentRenderProxy
   {
      public Effect Effect { get; private set; }
      public VertexBuffer VertexBuffer { get; private set; }
      public PrimitiveType PrimitiveType { get; private set; }
      public int PrimitiveCount { get; private set; }

      public Matrix WorldMatrix { get; set; }

      public ComponentRenderProxy(Effect setEffect, VertexBuffer setVertexBuffer,
                                  PrimitiveType setPrimitiveType, int setPrimitiveCount)
      {
         WorldMatrix = Matrix.Identity;
         Effect = setEffect;
         VertexBuffer = setVertexBuffer;
         PrimitiveType = setPrimitiveType;
         PrimitiveCount = setPrimitiveCount;
      }

      public void Render(IRenderConfiguration renderConfig)
      {
         renderConfig.GraphicsDevice.SetVertexBuffer(VertexBuffer);
         renderConfig.GraphicsDevice.DrawPrimitives(PrimitiveType, 0, PrimitiveCount);
      }
   }
}
