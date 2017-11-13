using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
   public abstract class BaseRenderProxy
   {
      public Effect Effect { get; private set; }

      public BaseRenderProxy(Effect setEffect)
      {
         Effect = setEffect;
      }

      public virtual void Render(IRenderConfiguration renderConfig, RenderManager renderManager)
      {
      }
   }
}
