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
    internal enum Ordering
    {
        Ignore,
        FrontToBack,
        BackToFront
    }

    internal class RenderProxyList
    {
        internal MaterialPhase IgnoreOrderingList = new MaterialPhase() { Ordering = Ordering.Ignore };
        internal MaterialPhase FrontToBackList = new MaterialPhase() { Ordering = Ordering.FrontToBack };
        internal MaterialPhase BackToFrontList = new MaterialPhase() { Ordering = Ordering.BackToFront };

        internal void Clear()
        {
            IgnoreOrderingList.Clear();
            FrontToBackList.Clear();
            BackToFrontList.Clear();
        }

        internal void AddProxy(StaticMeshRenderProxy proxy)
        {
            if (proxy.Material.RequiresOrderingBackToFront)
            {
                BackToFrontList.AddProxy(proxy);
            } else
            {
                IgnoreOrderingList.AddProxy(proxy);
            }
        }

        internal void Render(RenderContext renderContext)
        {
            FrontToBackList.Render(renderContext);
            IgnoreOrderingList.Render(renderContext);
            BackToFrontList.Render(renderContext);
        }

        internal void Update(double deltaTime)
        {
            FrontToBackList.Update(deltaTime);
            IgnoreOrderingList.Update(deltaTime);
            BackToFrontList.Update(deltaTime);
        }

        internal void SetWorldOriginForDepthSorting(Vector3 setOrigin)
        {
            FrontToBackList.SetWorldOriginForDepthSorting(setOrigin);
            IgnoreOrderingList.SetWorldOriginForDepthSorting(setOrigin);
            BackToFrontList.SetWorldOriginForDepthSorting(setOrigin);
        }
    }

    internal class MaterialPhase
    {
        internal IMaterial Material;

        internal Vector3 WorldOriginForDepthSorting;

        internal Ordering Ordering = Ordering.Ignore;

        internal List<StaticMeshRenderProxy> Proxies = new List<StaticMeshRenderProxy>();

        internal void Clear()
        {
            Proxies.Clear();
        }

        internal void SetWorldOriginForDepthSorting(Vector3 setOrigin)
        {
            WorldOriginForDepthSorting = setOrigin;
        }

        private int GetInsertIndex(BaseRenderProxy proxy)
        {
            float distanceSqr = Vector3.DistanceSquared(proxy.WorldMatrix.Translation, WorldOriginForDepthSorting);

            for (int i = 0; i < Proxies.Count; i++)
            {
                float distance2Sqr = Vector3.DistanceSquared(Proxies[i].WorldMatrix.Translation, WorldOriginForDepthSorting);
                if (Ordering == Ordering.BackToFront &&
                    distanceSqr > distance2Sqr)
                {
                    return i;
                }
                else if (Ordering == Ordering.FrontToBack &&
                    distanceSqr <= distance2Sqr)
                {
                    return i;
                }
            }

            if (Ordering == Ordering.BackToFront)
            {
                return Proxies.Count;
            }
            else
            {
                return 0;
            }
        }

        internal void AddProxy(StaticMeshRenderProxy proxy)
        {
            if (Ordering == Ordering.Ignore)
            {
                Proxies.Add(proxy);
                return;
            }

            int insertLoc = GetInsertIndex(proxy);
            Proxies.Insert(insertLoc, proxy);
        }

        internal void Render(RenderContext renderContext)
        {
            IMaterial? lastMaterial = null;

            BoundingFrustum frustum = renderContext.RenderManager.ViewTarget.CameraFrustum;

            int renderedProxies = 0;
            for (int i = 0; i < Proxies.Count; i++)
            {
                BaseRenderProxy proxy = Proxies[i];

                /*if (maxRenderCount != -1 && renderCount > maxRenderCount)
                {
                    break;
                }*/

                if (frustum.Contains(proxy.BoundingBox) == ContainmentType.Disjoint)
                {
                    continue;
                }

                IMaterial activeMat = proxy.Material;
                if (lastMaterial != activeMat)
                {
                    lastMaterial = activeMat;

                    lastMaterial.SetupMaterialForRenderPass(renderContext);
                }

                proxy.Render(renderContext, activeMat);

                //renderCount++;
                renderedProxies++;
            }
        }

        internal void Update(double deltaTime)
        {
            for (int i = 0; i < Proxies.Count; i++)
            {
                Proxies[i].Update(deltaTime);
            }
        }
    }

    class StaticMeshRenderPass
    {
        private object lockObj = new object();

        private RenderProxyList ActiveRenderProxyList = new RenderProxyList();
        private RenderProxyList StagingRenderProxyList = new RenderProxyList();

        internal StaticMeshRenderPass()
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

        internal void EnqueueRenderProxy(StaticMeshRenderProxy renderProxy)
        {
            StagingRenderProxyList.AddProxy(renderProxy);
        }

        internal void Render(RenderContext renderContext)
        {
            if (renderContext.RenderManager.ViewTarget == null)
            {
                throw new InvalidOperationException("A ViewTarget is required for rendering");
            }

            Performance.PushAggregate("Setup");
            Performance.PushAggregate("Setup Texture");
            Performance.PushAggregate("SetVertexBuffer");
            Performance.PushAggregate("DrawPrimitives");

            ActiveRenderProxyList.Render(renderContext);

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
