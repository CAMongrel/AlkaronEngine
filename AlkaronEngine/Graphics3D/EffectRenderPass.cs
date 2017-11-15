using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D.RenderProxies;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class EffectRenderPass
   {
      public Effect Effect { get; private set; }

      public bool PerformDepthSorting { get; set; }

      public Vector3 WorldOriginForDepthSorting { get; set; }

      private List<BaseRenderProxy> proxies;

      public EffectRenderPass(Effect setEffect)
      {
         WorldOriginForDepthSorting = Vector3.Zero;
         PerformDepthSorting = false;
         Effect = setEffect;
         proxies = new List<BaseRenderProxy>();
      }

      public void Clear()
      {
         proxies.Clear();
      }

      /// <summary>
      /// Returns the index in the proxies List for sorted inserting based
      /// on distance.
      /// 
      /// Note: Assumes a sorted list.
      /// </summary>
      private int GetInsertIndex(BaseRenderProxy proxy)
      {
         float distanceSqr = Vector3.DistanceSquared(proxy.WorldMatrix.Translation, WorldOriginForDepthSorting);

         for (int i = 0; i < proxies.Count; i++)
         {
            float distance2Sqr = Vector3.DistanceSquared(proxies[i].WorldMatrix.Translation, WorldOriginForDepthSorting);
            if (distanceSqr > distance2Sqr)
            {
               return i;
            }
         }

         return 0;
      }

      public void AddProxy(BaseRenderProxy proxy)
      {
         if (PerformDepthSorting == false)
         {
            proxies.Add(proxy);
            return;
         }

         int insertLoc = GetInsertIndex(proxy);
         proxies.Insert(insertLoc, proxy);
      }

      public void Draw(IRenderConfiguration renderConfig, RenderManager renderManager)
      {
         Performance.PushAggregate("Setup");
         Performance.PushAggregate("SetVertexBuffer");
         Performance.PushAggregate("DrawPrimitives");

         for (int i = 0; i < proxies.Count; i++)
         {
            proxies[i].Render(renderConfig, renderManager);
         }

         Performance.PopAggregate("DrawPrimitives");
         Performance.PopAggregate("SetVertexBuffer");
         Performance.PopAggregate("Setup");
      }
   }
}
