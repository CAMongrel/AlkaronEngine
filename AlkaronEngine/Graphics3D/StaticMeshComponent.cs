using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class StaticMeshComponent : BaseComponent
   {
      public StaticMesh StaticMesh { get; set; }

      private ComponentRenderProxy cachedProxy;

      public StaticMeshComponent(Vector3 setCenter)
         : base(setCenter)
      {
         CanBeRendered = true;
         cachedProxy = null;
      }

      public override ComponentRenderProxy Draw(GameTime gameTime, RenderManager renderManager)
      {
         if (cachedProxy == null)
         {
            cachedProxy = new ComponentRenderProxy(renderManager.EffectLibrary.EffectByName("StaticMesh"),
                                                   StaticMesh.VertexBuffer, StaticMesh.PrimitiveType,
                                                   StaticMesh.PrimitiveCount);
         }

         cachedProxy.WorldMatrix = Matrix.CreateTranslation(Center);

         return cachedProxy;
      }
   }
}
