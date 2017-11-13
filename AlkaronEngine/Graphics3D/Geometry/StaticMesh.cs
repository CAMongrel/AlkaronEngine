using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.Geometry
{
   public class StaticMesh
   {
      public VertexBuffer VertexBuffer { get; set; }
      public PrimitiveType PrimitiveType { get; set; }
      public int PrimitiveCount { get; set; }

      public Texture2D DiffuseTexture { get; set; }

      protected IRenderConfiguration renderConfig;

      public StaticMesh(IRenderConfiguration setRenderConfig)
      {
         renderConfig = setRenderConfig;
         DiffuseTexture = null;
      }
   }
}
