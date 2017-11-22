using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Components;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AlkaronEngine.Actors;
using AlkaronEngine.Graphics3D.RenderProxies;
using System.Threading;
using AlkaronEngine.Gui;

namespace AlkaronEngine.Graphics3D
{
    public class RenderManager
    {
        public bool SupportsMultiThreadedRendering { get { return false; } }

        private int maxFPS;

        private object lockObj;

        private Thread renderThread;
        private bool renderThreadActive;

        /// <summary>
        /// Needs locking with lockObj ... accessed by multiple threads.
        /// </summary>
        private List<BaseRenderProxy> renderProxyStage;

        private long frameRenderStartTime;

        public int MaxRenderCount;

        private int frameCounter;
        private long frameCalcStart;

        public float CurrentFPS { get; private set; }
        public int RenderedComponentsLastFrame { get; private set; }

        private RenderTarget2D renderTarget;
        private IRenderConfiguration renderConfig;
        private GraphicsDevice GraphicsDevice { get { return renderConfig?.GraphicsDevice; } }

        private List<RenderPass> renderPasses;

        public EffectLibrary EffectLibrary { get; private set; }

        public MaterialLibrary MaterialLibrary { get; private set; }

        internal ViewTarget ViewTarget { get; private set; }
        /// <summary>
        /// Applied to viewTarget at the start of the next frame
        /// Needs locking with lockObj ... accessed by multiple threads.
        /// </summary>
        private ViewTarget nextViewTarget;

        public MouseCursor MouseCursor { get; set; }

        public RenderManager(IRenderConfiguration setRenderConfig)
        {
            maxFPS = -1;

            ViewTarget = null;
            nextViewTarget = null;

            renderThread = null;
            renderThreadActive = false;
            lockObj = new Object();
            renderProxyStage = new List<BaseRenderProxy>();

            renderPasses = new List<RenderPass>();
            renderConfig = setRenderConfig;

            EffectLibrary = new EffectLibrary();
            MaterialLibrary = new MaterialLibrary();

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

            renderThread = new Thread(new ThreadStart(RenderThreadFunc));
            renderThread.Start();
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
            AlphaTestEffect eff = new AlphaTestEffect(renderConfig.GraphicsDevice);
            eff.FogEnabled = false;
            eff.VertexColorEnabled = false;
            /*eff.World = Matrix.Identity;
            eff.View = Matrix.CreateLookAt(new Vector3(0, 0, 15), Vector3.Zero, Vector3.Up);
            eff.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), renderConfig.GraphicsDevice.DisplayMode.AspectRatio, 1f, 1000f);*/
            EffectLibrary.AddEffect("StaticMesh", eff);

            BasicEffect eff2 = new BasicEffect(renderConfig.GraphicsDevice);
            eff2.FogEnabled = false;
            eff2.VertexColorEnabled = false;
            eff2.LightingEnabled = false;
            eff2.TextureEnabled = true;
            EffectLibrary.AddEffect("StaticMesh_Translucent", eff2);
        }

        private void CreateMaterialLibrary()
        {
        }

        private void CreateRenderTarget()
        {
            if (renderTarget != null)
            {
                renderTarget.Dispose();
                renderTarget = null;
            }

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

        public void AppendRenderProxies(List<BaseRenderProxy> list)
        {
            lock (lockObj)
            {
                renderProxyStage.AddRange(list);
            }
        }

        private void Clear(Color clearColor, ClearOptions options = ClearOptions.Target,
                             float clearDepth = 1.0f, int clearStencil = 0)
        {
            GraphicsDevice.Clear(options, clearColor, clearDepth, clearStencil);
        }

        private void RenderRenderPasses()
        {
            // Render 3D
            Clear(Color.Lavender, ClearOptions.DepthBuffer | ClearOptions.Stencil, 1, 0);

            Performance.Push("RenderManager.Draw (Set RenderTarget)");
            // Render scene to texture
            //renderConfig.GraphicsDevice.SetRenderTarget(renderTarget);
            Performance.Pop();

            // Perform rendering
            Performance.Push("RenderManager.Draw (Clear)");
            renderConfig.GraphicsDevice.Clear(Color.Black);
            Performance.Pop();

            Performance.Push("RenderManager.Draw (DrawRenderPasses)");
            DrawRenderPasses();
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

        private void RenderUI()
        {
            // Render 2D
            Performance.Push("BaseScene.Draw (Render2D)");
            // Clear depth buffer and stencil again for 2D rendering
            Clear(Color.Lavender, ClearOptions.DepthBuffer | ClearOptions.Stencil, 1, 0);
            UIWindowManager.Draw();
            MouseCursor?.Render();
            Performance.Pop();
        }

        // Main rendering function
        internal void RenderFrame(bool shouldSleep = false)
        {
            long end = System.Diagnostics.Stopwatch.GetTimestamp();
            double seconds = (double)(end - frameCalcStart) / (double)System.Diagnostics.Stopwatch.Frequency;
            if (seconds >= 1.0f)
            {
                frameCalcStart = end;
                CurrentFPS = frameCounter;

                frameCounter = 0;
            }

            if (maxFPS >= -1)
            {
                // Check maxFPS delay
                long now = System.Diagnostics.Stopwatch.GetTimestamp();
                double delta = (double)(now - frameRenderStartTime) / (double)System.Diagnostics.Stopwatch.Frequency;

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

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            ApplyPreFrame();

            RenderRenderPasses();

            RenderUI();

            Performance.Pop();

            frameCounter++;

            // Don't fry the CPU
            Thread.Sleep(1);
        }

        private void RenderThreadFunc()
        {
            while (renderThreadActive)
            {
                RenderFrame(true);
            }

            renderThread = null;
        }

        private void DrawRenderPasses()
        {
            int componentCount = 0;

            for (int i = 0; i < renderPasses.Count; i++)
            {
                componentCount += renderPasses[i].Draw(renderConfig, 
                    this, componentCount, MaxRenderCount);
            }

            RenderedComponentsLastFrame = componentCount;
        }

        public void ClearRenderPasses()
        {
            renderPasses.Clear();
        }

        public RenderPass CreateAndAddRenderPassForMaterial(Material material)
        {
            RenderPass renderPass = new RenderPass(material);
            renderPasses.Add(renderPass);
            return renderPass;
        }

        #region Pre-frame handling
        /// <summary>
        /// Must only be called from inside ApplyPreFrame() or otherwise secured with
        /// lock (lockObj).
        /// </summary>
        private void ApplyRenderProxyStage()
        {
            // Apply stage to current process

            Dictionary<Material, RenderPass> renderPassDict = new Dictionary<Material, RenderPass>();

            for (int p = 0; p < renderProxyStage.Count; p++)
            {
                BaseRenderProxy proxy = renderProxyStage[p];

                RenderPass passToUse = null;

                if (renderPassDict.ContainsKey(proxy.Material) == false)
                {
                    passToUse = CreateAndAddRenderPassForMaterial(proxy.Material);
                    renderPassDict.Add(proxy.Material, passToUse);
                }
                else
                {
                    passToUse = renderPassDict[proxy.Material];
                }

                passToUse.WorldOriginForDepthSorting = ViewTarget?.CameraLocation ?? Vector3.Zero;

                passToUse.AddProxy(proxy);
            }

            renderProxyStage.Clear();
        }

        private void ApplyPreFrame()
        {
            lock (lockObj)
            {
                if (nextViewTarget != null)
                {
                    ViewTarget = nextViewTarget;
                    nextViewTarget = null;
                }

                ApplyRenderProxyStage();
            }
        }
        #endregion
    }
}
