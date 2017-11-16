using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
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
         Vector3 cameraWorldLocation = renderManager.CameraLocation;

         renderManager.ClearRenderPasses();
         Dictionary<Material, RenderPass> renderPassDict = new Dictionary<Material, RenderPass>();

         for (int i = 0; i < Components.Count; i++)
         {
            if (Components[i].CanBeRendered == false)
            {
               continue;
            }

            if (renderManager.CameraFrustum.Contains(Components[i].BoundingBox) == ContainmentType.Disjoint)
            {
               continue;
            }

            BaseRenderProxy[] proxies = Components[i].Draw(gameTime, renderManager);
            if (proxies == null)
            {
               continue;
            }

            for (int p = 0; p < proxies.Length; p++)
            {
               BaseRenderProxy proxy = proxies[p];

               RenderPass passToUse = null;

               if (renderPassDict.ContainsKey(proxy.Material) == false)
               {
                  passToUse = renderManager.CreateAndAddRenderPassForMaterial(proxy.Material);
                  renderPassDict.Add(proxy.Material, passToUse);
               }
               else
               {
                  passToUse = renderPassDict[proxy.Material];
               }

               passToUse.WorldOriginForDepthSorting = cameraWorldLocation;

               passToUse.AddProxy(proxy);
            }
         }
      }
   }
}
