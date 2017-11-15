using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics3D.Components;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class SceneGraph
   {
      private List<BaseComponent> Components;

      public SceneGraph()
      {
         Components = new List<BaseComponent>();
      }

      public void Update(GameTime gameTime)
      {
         for (int i = 0; i < Components.Count; i++)
         {
            Components[i].Update(gameTime);
         }
      }

      public void AddComponent(BaseComponent newComponent)
      {
         Components.Add(newComponent);
      }

      public void Draw(GameTime gameTime, RenderManager renderManager)
      {
         bool performDepthSorting = false;

         Vector3 cameraWorldLocation = renderManager.CameraLocation;

         renderManager.ClearRenderPasses();
         Dictionary<Effect, EffectRenderPass> renderPassDict = new Dictionary<Effect, EffectRenderPass>();

         for (int i = 0; i < Components.Count; i++)
         {
            if (Components[i].CanBeRendered == false)
            {
               continue;
            }

            BaseRenderProxy proxy = Components[i].Draw(gameTime, renderManager);
            if (renderPassDict.ContainsKey(proxy.Effect) == false)
            {
               EffectRenderPass pass = renderManager.CreateAndAddRenderPassForEffect(proxy.Effect, performDepthSorting);
               if (pass.PerformDepthSorting)
               {
                  pass.WorldOriginForDepthSorting = cameraWorldLocation;
               }
               renderPassDict.Add(proxy.Effect, pass);
            }

            EffectRenderPass passToUse = renderPassDict[proxy.Effect];
            passToUse.AddProxy(proxy);
         }
      }
   }
}
