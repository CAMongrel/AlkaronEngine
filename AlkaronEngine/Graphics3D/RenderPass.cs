using System;
using System.Collections.Generic;
using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D.RenderProxies;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
    public class RenderPass
    {
        public Material Material { get; private set; }

        public bool PerformDepthSorting { get; set; }

        public Vector3 WorldOriginForDepthSorting { get; set; }

        private List<BaseRenderProxy> proxies;

        public RenderPass(Material setMaterial)
        {
            WorldOriginForDepthSorting = Vector3.Zero;
            PerformDepthSorting = setMaterial.RequiresOrderingBackToFront;
            Material = setMaterial;
            proxies = new List<BaseRenderProxy>();
        }

        public void Clear()
        {
            proxies.Clear();
        }

        /// <summary>
        /// Returns the index in the proxies List for sorted inserting based
        /// on distance.
        /// 
        /// Note: Assumes a sorted list.
        /// </summary>
        private int GetInsertIndex(BaseRenderProxy proxy)
        {
            float distanceSqr = Vector3.DistanceSquared(proxy.WorldMatrix.Translation, WorldOriginForDepthSorting);

            for (int i = 0; i < proxies.Count; i++)
            {
                float distance2Sqr = Vector3.DistanceSquared(proxies[i].WorldMatrix.Translation, WorldOriginForDepthSorting);
                if (distanceSqr > distance2Sqr)
                {
                    return i;
                }
            }

            return 0;
        }

        public void AddProxy(BaseRenderProxy proxy)
        {
            if (PerformDepthSorting == false)
            {
                proxies.Add(proxy);
                return;
            }

            int insertLoc = GetInsertIndex(proxy);
            proxies.Insert(insertLoc, proxy);
        }

        /// <summary>
        /// Draw all renderProxies using the specified renderConfig and renderManager.
        /// </summary>
        /// <returns>The draw.</returns>
        /// <param name="renderConfig">Render config.</param>
        /// <param name="renderManager">Render manager.</param>
        public int Draw(IRenderConfiguration renderConfig,
                        RenderManager renderManager,
                        int renderCount, 
                        int maxRenderCount)
        {
            BoundingFrustum frustum = renderManager.ViewTarget?.CameraFrustum;
            if (frustum == null)
            {
                return 0;
            }

            if (PerformDepthSorting == true)
            {
                Performance.Push("Perform DepthSorting");
                WorldOriginForDepthSorting = renderManager.ViewTarget?.CameraLocation ?? Vector3.Zero;
                proxies.Sort((x, y) => 
                {
                    float distanceSqr = Vector3.DistanceSquared(x.WorldMatrix.Translation, WorldOriginForDepthSorting);
                    float distance2Sqr = Vector3.DistanceSquared(y.WorldMatrix.Translation, WorldOriginForDepthSorting);
                    return distance2Sqr.CompareTo(distanceSqr);
                });
                Performance.Pop();
            }

            Performance.PushAggregate("Setup");
            Performance.PushAggregate("Setup Texture");
            Performance.PushAggregate("SetVertexBuffer");
            Performance.PushAggregate("DrawPrimitives");

            Material.SetupEffectForRenderPass(this);

            int renderedProxies = 0;
            for (int i = 0; i < proxies.Count; i++)
            {
                if (maxRenderCount != -1 && renderCount > maxRenderCount)
                {
                    break;
                }

                if (frustum.Contains(proxies[i].BoundingBox) == ContainmentType.Disjoint)
                {
                    continue;
                }
                proxies[i].Render(renderConfig, renderManager, Material);

                renderCount++;
                renderedProxies++;
            }

            Performance.PopAggregate("DrawPrimitives");
            Performance.PopAggregate("SetVertexBuffer");
            Performance.PopAggregate("Setup Texture");
            Performance.PopAggregate("Setup");

            return renderedProxies;
        }

        internal void Update(double deltaTime)
        {
            for (int i = 0; i < proxies.Count; i++)
            {
                proxies[i].Update(deltaTime);
            }
        }
    }
}
