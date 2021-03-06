using System;
using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Components;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
    public abstract class BaseRenderProxy
    {
        /// <summary>
        /// Lock object for cross-thread communication. Every action that 
        /// potentially crosses thread-boundaries must be guarded with this
        /// lock object.
        /// </summary>
        protected object lockObj = new Object();

        public IMaterial Material { get; set; }
        public Matrix WorldMatrix { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public BaseRenderProxy()
        {
            Material = null;
            WorldMatrix = Matrix.Identity;
        }

        public virtual void Render(IRenderConfiguration renderConfig, RenderManager renderManager, IMaterial materialToUse)
        {
        }

        /// <summary>
        /// Called during the rendering process and on the rendering thread
        /// (if rendering thread is used)
        /// </summary>
        internal virtual void Update(double deltaTime)
        {
            //
        }
    }
}
