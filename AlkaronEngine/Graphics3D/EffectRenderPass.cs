using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class EffectRenderPass
   {
      public Effect Effect { get; private set; }

      private List<ComponentRenderProxy> proxies;

      public EffectRenderPass(Effect setEffect)
      {
         Effect = setEffect;
         proxies = new List<ComponentRenderProxy>();
      }

      public void Clear()
      {
         proxies.Clear();
      }

      public void AddProxy(ComponentRenderProxy proxy)
      {
         proxies.Add(proxy);
      }

      public void Draw(IRenderConfiguration renderConfig, RenderManager renderManager)
      {
         for (int i = 0; i < proxies.Count; i++)
         {
            Effect.Parameters["WorldViewProj"].SetValue(proxies[i].WorldMatrix * renderManager.ViewMatrix * renderManager.ProjectionMatrix);
            Effect.CurrentTechnique.Passes[0].Apply();

            proxies[i].Render(renderConfig);
         }
      }
   }
}
