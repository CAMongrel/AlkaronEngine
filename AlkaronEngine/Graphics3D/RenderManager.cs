using System;
using System.Collections.Generic;
using AlkaronEngine.Components;
using AlkaronEngine.Util;
using AlkaronEngine.Graphics3D.RenderProxies;
using System.Threading;
using AlkaronEngine.Assets.Materials;
using Veldrid;
using System.Numerics;
using AlkaronEngine.Gui;
using Veldrid.SPIRV;
using System.Text;
using AlkaronEngine.Graphics3D.RenderPasses;

namespace AlkaronEngine.Graphics3D
{
    public class RenderContext
    {
        public CommandList CommandList;
        public GraphicsDevice GraphicsDevice;
        public RenderManager RenderManager;

        // Stats
        public long RenderedTrianglesThisFrame;
    }

    public class RenderManager
    {
        public bool SupportsMultiThreadedRendering { get { return false; } }

        private int maxFPS;

        private object lockObj;

        private Thread renderThread;
        private bool renderThreadActive;

        private long frameRenderStartTime;

        public int MaxRenderCount;

        private int frameCounter;
        private long frameCalcStart;

        // Public stats
        public float CurrentFPS { get; private set; }
        public int RenderedComponentsLastFrame { get; private set; }

        internal ViewTarget? ViewTarget { get; private set; }

        /// <summary>
        /// Applied to viewTarget at the start of the next frame
        /// Needs locking with lockObj ... accessed by multiple threads.
        /// </summary>
        private ViewTarget? nextViewTarget;

        private StaticMeshRenderPass staticMeshRenderPass;

        public RenderManager()
        {
            maxFPS = -1;

            ViewTarget = null;
            nextViewTarget = null;

            renderThread = null;
            renderThreadActive = false;
            lockObj = new Object();
            //renderProxyStagingArea = new List<BaseRenderProxy>();

            //renderPasses = new List<RenderPass>();
            staticMeshRenderPass = new StaticMeshRenderPass();

            CreateRenderTarget();
            CreateEffectLibrary();
            CreateMaterialLibrary();

            frameCounter = 0;
            frameCalcStart = System.Diagnostics.Stopwatch.GetTimestamp();

            MaxRenderCount = -1;
        }

        public void Start()
        {
            if (SupportsMultiThreadedRendering == false)
            {
                return;
            }

            if (renderThread != null)
            {
                return;
            }

            frameRenderStartTime = System.Diagnostics.Stopwatch.GetTimestamp();

            renderThreadActive = true;

            //renderThread = new Thread(new ThreadStart(RenderThreadFunc));
            //renderThread.Start();
        }

        public void Stop()
        {
            if (SupportsMultiThreadedRendering == false)
            {
                return;
            }

            renderThreadActive = false;
        }

        private void CreateEffectLibrary()
        {
            
        }

        private void CreateMaterialLibrary()
        {
        }

        private void CreateRenderTarget()
        {
            /*if (renderTarget != null)
            {
                renderTarget.Dispose();
                renderTarget = null;
            }*/

            /*renderTarget = new RenderTarget2D(renderConfig.GraphicsDevice,
                                              (int)renderConfig.ScreenSize.X,
                                              (int)renderConfig.ScreenSize.Y,
                                              false,
                                              SurfaceFormat.Color,
                                              DepthFormat.Depth24,
                                              0,
                                              RenderTargetUsage.PlatformContents,
                                              false,
                                              0);*/
        }

        internal void SetWorldOriginForDepthSorting(CameraComponent cameraComponent)
        {
            staticMeshRenderPass.SetWorldOriginForDepthSorting(cameraComponent.Center);
        }

        public void SizeChanged()
        {
            CreateRenderTarget();
        }

        public void SetViewTargetFromCameraComponent(CameraComponent cameraComponent)
        {
            lock (lockObj)
            {
                nextViewTarget = new ViewTarget(cameraComponent.Center, cameraComponent.ViewMatrix, cameraComponent.ProjectionMatrix);
            }
        }

        internal void EnqueueRenderProxy(BaseRenderProxy proxy)
        {
            if (proxy is StaticMeshRenderProxy smrr)
            {
                staticMeshRenderPass.EnqueueRenderProxy(smrr);
            }
            else
            {
                Log.Error("Cannot handle proxy of type: " + proxy?.GetType() ?? "null");
            }
        }

        private void Clear(RenderContext renderContext, RgbaFloat clearColor, bool clearDepthStencil = false,
                             float clearDepth = 1.0f, byte clearStencil = 0)
        {
            renderContext.CommandList.ClearColorTarget(0, clearColor);
            if (clearDepthStencil)
            {
                renderContext.CommandList.ClearDepthStencil(clearDepth, clearStencil);
            }
        }

        private void RenderRenderPasses(RenderContext renderContext)
        {
            // Render 3D
            Clear(renderContext, RgbaFloat.Green, true, 1, 0);

            Performance.Push("RenderManager.Draw (Set RenderTarget)");
            // Render scene to texture
            //renderConfig.GraphicsDevice.SetRenderTarget(renderTarget);
            Performance.Pop();

            // Perform rendering
            Performance.Push("RenderManager.Draw (Clear)");
            Clear(renderContext, RgbaFloat.CornflowerBlue);
            Performance.Pop();

            Performance.Push("RenderManager.Draw (DrawRenderPasses)");
            DrawRenderPasses(renderContext);
            Performance.Pop();

            // Reset renderTarget
            Performance.Push("RenderManager.Draw (SetRenderTarget)");
            //renderConfig.GraphicsDevice.SetRenderTarget(null);
            Performance.Pop();

            // Draw renderTarget to screen
            Performance.Push("RenderManager.Draw (RenderQuad)");
            /*ScreenQuad.RenderQuad(Vector2.Zero, renderConfig.ScreenSize,
                                  Color.White, 0.0f, renderTarget,
                                  renderConfig);*/
            Performance.Pop();
        }

        private void RenderUI(RenderContext renderContext)
        {
            // Render 2D
            Performance.Push("BaseScene.Draw (Render2D)");
            // Clear depth buffer and stencil again for 2D rendering
            //Clear(Color.Lavender, ClearOptions.DepthBuffer | ClearOptions.Stencil, 1, 0);
            UIWindowManager.Draw(renderContext);
            //MouseCursor?.Render();
            Performance.Pop();
        }

        // Main rendering function
        internal void RenderFrame(RenderContext renderContext, bool shouldSleep = false)
        {
            long end = System.Diagnostics.Stopwatch.GetTimestamp();
            double seconds = (double)(end - frameCalcStart) / (double)System.Diagnostics.Stopwatch.Frequency;
            if (seconds >= 1.0f)
            {
                frameCalcStart = end;
                CurrentFPS = frameCounter;

                Console.WriteLine("CurrentFPS: " + CurrentFPS);

                frameCounter = 0;
            }

            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            // Delta time in seconds since the last rendering pass
            double delta = (double)(now - frameRenderStartTime) / (double)System.Diagnostics.Stopwatch.Frequency;

            if (maxFPS >= -1)
            {
                // Check maxFPS delay

                double minDelta = 1.0 / (double)maxFPS;
                if (delta < minDelta)
                {
                    if (shouldSleep)
                    {
                        Thread.Sleep(1);
                    }
                    return;
                }
            }

            frameRenderStartTime = System.Diagnostics.Stopwatch.GetTimestamp();

            Performance.Push("RenderManager.Draw");

            // Apply update before rendering (adding new rendering proxies,
            // updating existing proxies)
            ApplyPreFrame(delta);

            // Render all active proxies in rendering passes
            RenderRenderPasses(renderContext);

            // Render the UI
            RenderUI(renderContext);

            Performance.Pop();

            frameCounter++;

            // Don't fry the CPU
            Thread.Sleep(1);
        }

        private void RenderThreadFunc(RenderContext renderContext)
        {
            while (renderThreadActive)
            {
                RenderFrame(renderContext, true);
            }

            renderThread = null;
        }

        private void DrawRenderPasses(RenderContext renderContext)
        {
            int componentCount = 0;

            // Depth passes for lights/shadows
            IMaterial? depthMaterial = null;
            //staticMeshRenderPass.Render(renderContext, depthMaterial);

            // Color pass
            staticMeshRenderPass.Render(renderContext);
        }

        #region Pre-frame handling
        private void ApplyPreFrame(double deltaTime)
        {
            lock (lockObj)
            {
                if (nextViewTarget != null)
                {
                    ViewTarget = nextViewTarget;
                    nextViewTarget = null;
                }

                staticMeshRenderPass.SwapListsAndClearStage();
            }

            UpdateRenderProxies(deltaTime);
        }

        private void UpdateRenderProxies(double deltaTime)
        {
            staticMeshRenderPass.Update(deltaTime);
        }
        #endregion
    }
}
