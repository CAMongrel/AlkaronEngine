using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Graphics3D.RenderProxies;
using AlkaronEngine.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid.Utilities;

namespace AlkaronEngine.Graphics3D.RenderPasses
{
    class SkeletalMeshRenderPass
    {
        private object lockObj = new object();

        private RenderProxyList ActiveRenderProxyList = new RenderProxyList();
        private RenderProxyList StagingRenderProxyList = new RenderProxyList();

        internal SkeletalMeshRenderPass()
        {
        }

        internal void SwapListsAndClearStage()
        {
            lock (lockObj)
            {
                RenderProxyList temp = StagingRenderProxyList;
                StagingRenderProxyList = ActiveRenderProxyList;
                ActiveRenderProxyList = temp;

                StagingRenderProxyList.Clear();
            }
        }

        internal void SetWorldOriginForDepthSorting(Vector3 setOrigin)
        {
            StagingRenderProxyList.SetWorldOriginForDepthSorting(setOrigin);
        }

        internal void EnqueueRenderProxy(SkeletalMeshRenderProxy renderProxy)
        {
            StagingRenderProxyList.AddProxy(renderProxy);
        }

        internal void Render(RenderContext renderContext, IMaterial? materialOverride = null)
        {
            if (renderContext.RenderManager.ViewTarget == null)
            {
                throw new InvalidOperationException("A ViewTarget is required for rendering");
            }

            Performance.PushAggregate("Setup");
            Performance.PushAggregate("Setup Texture");
            Performance.PushAggregate("SetVertexBuffer");
            Performance.PushAggregate("DrawPrimitives");

            ActiveRenderProxyList.Render(renderContext, materialOverride);

            Performance.PopAggregate("DrawPrimitives");
            Performance.PopAggregate("SetVertexBuffer");
            Performance.PopAggregate("Setup Texture");
            Performance.PopAggregate("Setup");

            // Clear active list after rendering
            ActiveRenderProxyList.Clear();
        }

        internal void Update(double deltaTime)
        {
            ActiveRenderProxyList.Update(deltaTime);
        }
    }
}
