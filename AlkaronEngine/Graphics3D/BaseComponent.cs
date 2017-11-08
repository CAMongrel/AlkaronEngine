using System;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Graphics3D
{
   public abstract class BaseComponent
   {
      public Vector3 Center { get; private set; }

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

      public virtual ComponentRenderProxy Draw(GameTime gameTime, RenderManager renderManager)
      {
         return null;
      }
   }
}