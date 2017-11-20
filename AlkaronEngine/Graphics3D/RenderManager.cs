using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D.Components;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
    public class RenderManager
    {
        private int frameCounter;
        private long frameCalcStart;

        public float CurrentFPS { get; private set; }
        public int RenderedComponentsLastFrame { get; private set; }

        private RenderTarget2D renderTarget;
        private IRenderConfiguration renderConfig;

        private List<RenderPass> renderPasses;

        public Vector3 CameraLocation { get; private set; }
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }
        public BoundingFrustum CameraFrustum { get; private set; }

        public EffectLibrary EffectLibrary { get; private set; }

        public MaterialLibrary MaterialLibrary { get; private set; }

        public RenderManager(IRenderConfiguration setRenderConfig)
        {
            renderPasses = new List<RenderPass>();
            renderConfig = setRenderConfig;

            EffectLibrary = new EffectLibrary();
            MaterialLibrary = new MaterialLibrary();

            CreateRenderTarget();
            CreateEffectLibrary();
            CreateMaterialLibrary();

            frameCounter = 0;
            frameCalcStart = System.Diagnostics.Stopwatch.GetTimestamp();
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

            renderTarget = new RenderTarget2D(renderConfig.GraphicsDevice,
                                              (int)renderConfig.ScreenSize.X,
                                              (int)renderConfig.ScreenSize.Y,
                                              false,
                                              SurfaceFormat.Color,
                                              DepthFormat.Depth24,
                                              0,
                                              RenderTargetUsage.PlatformContents,
                                              false,
                                              0);
        }

        public void SizeChanged()
        {
            CreateRenderTarget();
        }

        public void UpdateMatricesFromCameraComponent(CameraComponent camera)
        {
            CameraLocation = camera.Center;
            ViewMatrix = camera.ViewMatrix;
            ProjectionMatrix = camera.ProjectionMatrix;

            CameraFrustum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);
        }

        public void Draw(GameTime gameTime)
        {
            Performance.Push("RenderManager.Draw");

            Performance.Push("RenderManager.Draw (Set RenderTarget)");
            // Render scene to texture
            renderConfig.GraphicsDevice.SetRenderTarget(renderTarget);
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
            renderConfig.GraphicsDevice.SetRenderTarget(null);
            Performance.Pop();

            // Draw renderTarget to screen
            Performance.Push("RenderManager.Draw (RenderQuad)");
            ScreenQuad.RenderQuad(Vector2.Zero, renderConfig.ScreenSize,
                                  Color.White, 0.0f, renderTarget,
                                  renderConfig);
            Performance.Pop();
            Performance.Pop();

            frameCounter++;

            long end = System.Diagnostics.Stopwatch.GetTimestamp();
            double seconds = (double)(end - frameCalcStart) / (double)System.Diagnostics.Stopwatch.Frequency;
            if (seconds >= 1.0f)
            {
                frameCalcStart = end;
                CurrentFPS = frameCounter;

                frameCounter = 0;
            }
        }

        private void DrawRenderPasses()
        {
            int componentCount = 0;

            for (int i = 0; i < renderPasses.Count; i++)
            {
                componentCount += renderPasses[i].Draw(renderConfig, this);
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
    }
}
