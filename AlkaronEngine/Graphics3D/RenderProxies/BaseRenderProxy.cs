using System;
using System.Numerics;
using AlkaronEngine.Assets.Materials;
using Veldrid.Utilities;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
    internal enum RenderProxyType
    {
        StaticMesh,
        SkeletalMesh
    }
    
    internal abstract class BaseRenderProxy
    {
        /// <summary>
        /// Lock object for cross-thread communication. Every action that 
        /// potentially crosses thread-boundaries must be guarded with this
        /// lock object.
        /// </summary>
        protected object lockObj = new Object();

        public IMaterial Material { get; set; }
        public Matrix4x4 WorldMatrix { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public RenderProxyType Type { get; protected set; }

        public BaseRenderProxy()
        {
            Material = null;
            WorldMatrix = Matrix4x4.Identity;
        }

        public virtual void Render(RenderContext renderContext, IMaterial materialToUse)
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
