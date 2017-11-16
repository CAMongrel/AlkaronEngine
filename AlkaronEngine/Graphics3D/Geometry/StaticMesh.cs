using System;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.Geometry
{
   public class StaticMesh
   {
      public Material Material { get; set; }

      public VertexBuffer VertexBuffer { get; set; }
      public PrimitiveType PrimitiveType { get; set; }
      public int PrimitiveCount { get; set; }

      public Texture2D DiffuseTexture { get; set; }

      public bool IsCollisionOnly { get; set; }

      protected IRenderConfiguration renderConfig;

      public BoundingBox BoundingBox { get; set; }

      public StaticMesh(IRenderConfiguration setRenderConfig)
      {
         IsCollisionOnly = false;
         renderConfig = setRenderConfig;
         DiffuseTexture = null;
         Material = null;
      }
   }
}
