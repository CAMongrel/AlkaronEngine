using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class EffectRenderPass
   {
      public Effect Effect { get; private set; }

      private List<BaseRenderProxy> proxies;

      public EffectRenderPass(Effect setEffect)
      {
         Effect = setEffect;
         proxies = new List<BaseRenderProxy>();
      }

      public void Clear()
      {
         proxies.Clear();
      }

      public void AddProxy(BaseRenderProxy proxy)
      {
         proxies.Add(proxy);
      }

      public void Draw(IRenderConfiguration renderConfig, RenderManager renderManager)
      {
         for (int i = 0; i < proxies.Count; i++)
         {
            proxies[i].Render(renderConfig, renderManager);
         }
      }
   }
}
