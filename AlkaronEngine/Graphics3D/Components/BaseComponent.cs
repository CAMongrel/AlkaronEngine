using System;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Graphics3D.Components
{
   public abstract class BaseComponent
   {
      public Vector3 Center { get; protected set; }

      public bool CanBeRendered { get; protected set; }

      public BaseComponent(Vector3 setCenter)
      {
         Center = setCenter;
         CanBeRendered = false;
      }

      public virtual void Update(GameTime gameTime)
      {
         //
      }

      public virtual BaseRenderProxy Draw(GameTime gameTime, RenderManager renderManager)
      {
         return null;
      }
   }
}