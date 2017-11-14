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
      public List<StaticMesh> StaticMeshes { get; set; }

      private StaticMeshComponentRenderProxy cachedProxy;

      public StaticMeshComponent(Vector3 setCenter)
         : base(setCenter)
      {
         CanBeRendered = true;
         cachedProxy = null;
         StaticMeshes = new List<StaticMesh>();
      }

      public override BaseRenderProxy Draw(GameTime gameTime, RenderManager renderManager)
      {
         if (cachedProxy == null)
         {
            cachedProxy = new StaticMeshComponentRenderProxy(renderManager.EffectLibrary.EffectByName("StaticMesh"),
                                                             this);
         }

         cachedProxy.WorldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) * 
            Matrix.CreateScale(Scale.X, Scale.Y, Scale.Z) * 
            Matrix.CreateTranslation(Center);

         return cachedProxy;
      }
   }
}
