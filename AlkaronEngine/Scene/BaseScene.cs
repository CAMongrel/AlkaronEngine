using System.Diagnostics.Contracts;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Gui;
using AlkaronEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Scene
{
   public class BaseScene
   {
      public static Color StandardBackgroundColor = new Color(0x7F, 0x00, 0x00);

      public MouseCursor MouseCursor { get; set; }

      public IRenderConfiguration RenderConfig { get; private set; } 

      public ContentManager ContentManager { get; private set; }

      public SceneGraph SceneGraph { get; private set; }

      public RenderManager RenderManager { get; private set; }

      public CameraComponent CurrentCamera { get; set; }

      public BaseScene()
      {
         
      }

      public virtual Color BackgroundColor
      {
         get
         {
            return StandardBackgroundColor;
         }
      }

      public virtual void Init(IRenderConfiguration setRenderConfig)
      {
         Contract.Requires(setRenderConfig != null);

         RenderConfig = setRenderConfig;
         ContentManager = new ContentManager(AlkaronCoreGame.Core.Content.ServiceProvider, "Content");
         SceneGraph = new SceneGraph();
         RenderManager = new RenderManager(RenderConfig);

         Init3D();
         InitUI();
      }

      internal void ClientSizeChanged()
      {
         RenderManager.SizeChanged();
         UIWindowManager.PerformLayout();
      }

      protected virtual void InitUI()
      {
         //
      }

      protected virtual void Init3D()
      {
         // Create default camera
         CurrentCamera = new CameraComponent(new Vector3(0, 0, 15),
                                             Vector3.Up,
                                             Vector3.Zero,
                                             RenderConfig.ScreenSize,
                                             0.1f,
                                             500.0f);
         SceneGraph.AddComponent(CurrentCamera);
      }

      public virtual void Close()
      {
         UIWindowManager.Clear();

         ContentManager.Unload();
         ContentManager.Dispose();
         ContentManager = null;
      }

      public virtual void Update(GameTime gameTime)
      {
         // Update 3D
         SceneGraph.Update(gameTime);

         // Update 2D
         UIWindowManager.Update(gameTime);
      }

      protected void Clear(Color clearColor, ClearOptions options = ClearOptions.Target,
                           float clearDepth = 1.0f, int clearStencil = 0)
      {
         RenderConfig.GraphicsDevice.Clear(options, clearColor, clearDepth, clearStencil);
      }

      public virtual void Draw(GameTime gameTime)
      {
         // Render 3D
         SceneGraph.Draw(gameTime, RenderManager);       // SceneGraph.Draw() only creates the RenderProxies
         // Clear only the depth and stencil buffer
         Clear(BackgroundColor, ClearOptions.DepthBuffer | ClearOptions.Stencil, 1, 0);
         RenderManager.UpdateMatricesFromCameraComponent(CurrentCamera);
         RenderManager.Draw(gameTime);

         // Render 2D
         // Clear depth buffer and stencil again for 2D rendering
         Clear(BackgroundColor, ClearOptions.DepthBuffer | ClearOptions.Stencil, 1, 0);
         UIWindowManager.Draw(gameTime);
         MouseCursor?.Render(gameTime);
      }

      public virtual void PointerDown(Vector2 position, PointerType pointerType)
      {
         UIWindowManager.PointerDown(position, pointerType);
      }

      public virtual void PointerUp(Vector2 position, PointerType pointerType)
      {
         UIWindowManager.PointerUp(position, pointerType);
      }

      public virtual void PointerMoved(Vector2 position)
      {
         UIWindowManager.PointerMoved(position);
      }
   }
}
