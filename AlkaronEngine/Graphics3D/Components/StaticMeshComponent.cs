using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.Components
{
   public class StaticMeshComponent : BaseComponent
   {
      private List<StaticMeshRenderProxy> cachedProxies;
      public List<StaticMesh> StaticMeshes { get; set; }

      public StaticMeshComponent(Vector3 setCenter)
         : base(setCenter)
      {
         CanBeRendered = true;
         StaticMeshes = new List<StaticMesh>();
         cachedProxies = new List<StaticMeshRenderProxy>();
      }

      private StaticMeshRenderProxy GetRenderProxy(StaticMesh mesh)
      {
         for (int i = 0; i < cachedProxies.Count; i++)
         {
            if (cachedProxies[i].StaticMesh == mesh)
            {
               return cachedProxies[i];
            }
         }

         StaticMeshRenderProxy cachedProxy = new StaticMeshRenderProxy(mesh);
         cachedProxies.Add(cachedProxy);
         return cachedProxy;
      }

      public override BaseRenderProxy[] Draw(GameTime gameTime, RenderManager renderManager)
      {
         if (StaticMeshes.Count == 0)
         {
            return null;
         }

         Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
            Matrix.CreateScale(Scale.X, Scale.Y, Scale.Z) *
            Matrix.CreateTranslation(Center);

         for (int i = 0; i < StaticMeshes.Count; i++)
         {
            StaticMesh mesh = StaticMeshes[i];

            StaticMeshRenderProxy proxy = GetRenderProxy(mesh);
            proxy.WorldMatrix = worldMatrix;
            proxy.Material = mesh.Material;
         }

         return cachedProxies.ToArray();
      }
   }
}
