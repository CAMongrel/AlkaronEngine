using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Graphics3D.RenderProxies;
using AlkaronEngine.Scene;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
    public abstract class BaseComponent
    {
        public Vector3 Center { get; protected set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public BoundingBox BoundingBox { get; protected set; }

        public bool CanBeRendered { get; protected set; }

        public bool IsDirty { get; protected set; }

        protected BaseRenderProxy[] renderProxies;

        public BaseComponent(Vector3 setCenter)
        {
            renderProxies = null;

            Center = setCenter;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
            CanBeRendered = false;
            IsDirty = true;
            BoundingBox = new BoundingBox();
        }

        public virtual void Update(GameTime gameTime)
        {
            //
        }

        public void SetCenter(Vector3 newCenter, bool performSweep)
        {
            Center = newCenter;
            IsDirty = true;
        }

        public virtual IEnumerable<BaseRenderProxy> GetRenderProxies()
        {
            return renderProxies;
        }

        public virtual void OwnerActorAddedToSceneGraph(SceneGraph owner)
        {
            //
        }
    }
}