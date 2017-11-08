using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class RenderManager
   {
      private RenderTarget2D renderTarget;
      private IRenderConfiguration renderConfig;

      private List<EffectRenderPass> renderPasses;

      public Matrix ViewMatrix { get; private set; }
      public Matrix ProjectionMatrix { get; private set; }

      public EffectLibrary EffectLibrary { get; private set; }

      public RenderManager(IRenderConfiguration setRenderConfig)
      {
         renderPasses = new List<EffectRenderPass>();
         renderConfig = setRenderConfig;

         EffectLibrary = new EffectLibrary();

         CreateRenderTarget();
         CreateEffectLibrary();
      }

      private void CreateEffectLibrary()
      {
         BasicEffect eff = new BasicEffect(renderConfig.GraphicsDevice);
         eff.FogEnabled = false;
         eff.LightingEnabled = false;
         eff.VertexColorEnabled = true;
         /*eff.World = Matrix.Identity;
         eff.View = Matrix.CreateLookAt(new Vector3(0, 0, 15), Vector3.Zero, Vector3.Up);
         eff.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), renderConfig.GraphicsDevice.DisplayMode.AspectRatio, 1f, 1000f);*/
         EffectLibrary.AddEffect("StaticMesh", eff);
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
         ViewMatrix = camera.ViewMatrix;
         ProjectionMatrix = camera.ProjectionMatrix;
      }

      public void Draw(GameTime gameTime)
      {
         // Render scene to texture
         renderConfig.GraphicsDevice.SetRenderTarget(renderTarget);

         // Perform rendering
         renderConfig.GraphicsDevice.Clear(Color.LightGreen);

         RenderPasses();

         // Reset renderTarget
         renderConfig.GraphicsDevice.SetRenderTarget(null);

         // Draw renderTarget to screen
         ScreenQuad.RenderQuad(Vector2.Zero, renderConfig.ScreenSize,
                               Color.White, 0.0f, renderTarget,
                               renderConfig);
      }

      private void RenderPasses()
      {
         for (int i = 0; i < renderPasses.Count; i++)
         {
            renderPasses[i].Draw(renderConfig, this);
         }
      }

      public void ClearRenderPasses()
      {
         renderPasses.Clear();
      }

      public EffectRenderPass CreateRenderPassForEffect(Effect effect)
      {
         EffectRenderPass renderPass = new EffectRenderPass(effect);
         renderPasses.Add(renderPass);
         return renderPass;
      }
   }
}
