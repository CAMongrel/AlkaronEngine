using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
   public abstract class BaseRenderProxy
   {
      public Material Material { get; set; }
      public Matrix WorldMatrix { get; set; }

      public BaseRenderProxy()
      {
         Material = null;
         WorldMatrix = Matrix.Identity;
      }

      public virtual void Render(IRenderConfiguration renderConfig, RenderManager renderManager)
      {
      }
   }
}
