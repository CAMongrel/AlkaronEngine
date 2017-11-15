using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
   public abstract class BaseRenderProxy
   {
      public Effect Effect { get; private set; }
      public Matrix WorldMatrix { get; set; }

      public BaseRenderProxy(Effect setEffect)
      {
         Effect = setEffect;
         WorldMatrix = Matrix.Identity;
      }

      public virtual void Render(IRenderConfiguration renderConfig, RenderManager renderManager)
      {
      }
   }
}
