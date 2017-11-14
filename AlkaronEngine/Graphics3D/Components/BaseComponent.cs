using System;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Graphics3D.Components
{
   public abstract class BaseComponent
   {
      public Vector3 Center { get; protected set; }
      public Vector3 Rotation { get; set; }
      public Vector3 Scale { get; set; }

      public bool CanBeRendered { get; protected set; }

      public BaseComponent(Vector3 setCenter)
      {
         Center = setCenter;
         Rotation = Vector3.Zero;
         Scale = Vector3.One;
         CanBeRendered = false;
      }

      public virtual void Update(GameTime gameTime)
      {
         //
      }

      public void SetCenter(Vector3 newCenter, bool performSweep)
      {
         Center = newCenter;
      }

      public virtual BaseRenderProxy Draw(GameTime gameTime, RenderManager renderManager)
      {
         return null;
      }
   }
}