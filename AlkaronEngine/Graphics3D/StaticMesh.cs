using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class StaticMesh
   {
      public VertexBuffer VertexBuffer { get; protected set; }
      public PrimitiveType PrimitiveType { get; protected set; }
      public int PrimitiveCount { get; protected set; }

      protected IRenderConfiguration renderConfig;

      public StaticMesh(IRenderConfiguration setRenderConfig)
      {
         renderConfig = setRenderConfig;
      }
   }
}
