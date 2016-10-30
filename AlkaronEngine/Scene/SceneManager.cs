using AlkaronEngine.Graphics;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AlkaronEngine.Scene
{
   public class SceneManager : IRenderConfiguration
   {
      public BaseScene CurrentScene { get; private set; }
      public BaseScene NextScene { get; set; }

      public GraphicsDevice GraphicsDevice { get; private set; }
      public InputManager InputManager { get; private set; }
      public RenderManager RenderManager { get; private set; }

      public virtual Vector2 Scale
      {
         get
         {
            return new Vector2(1, 1);
         }
      }

      public virtual Vector2 ScaledOffset
      {
         get
         {
            return new Vector2(0, 0);
         }
      }

      public bool RequiresPowerOfTwoTextures
      {
         get
         {
            return false;
         }
      }

      public Vector2 ScreenSize
      {
         get
         {
            return new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
         }
      }

      public SceneManager(GraphicsDevice setGraphicsDevice)
      {
         if (setGraphicsDevice == null)
         {
            throw new ArgumentNullException(nameof(setGraphicsDevice));
         }
         GraphicsDevice = setGraphicsDevice;

         RenderManager = new RenderManager(this);
         InputManager = new InputManager(this);

         CurrentScene = null;
         NextScene = null;
      }

      public void Update(GameTime gameTime)
      {
         Performance.Push("Game loop");
         InputManager.UpdateInput(gameTime);

         if (NextScene != null)
         {
            if (CurrentScene != null)
            {
               CurrentScene.Close();
            }

            CurrentScene = NextScene;
            if (CurrentScene != null)
            {
               CurrentScene.Init();
            }

            NextScene = null;
         }

         if (CurrentScene != null)
         {
            CurrentScene.Update(gameTime);
         }

         Performance.Pop();
      }

      public void Draw(GameTime gameTime)
      {
         Performance.Push("Render loop");
         if (CurrentScene != null)
         {
            GraphicsDevice.Clear(CurrentScene.BackgroundColor);

            CurrentScene.Draw(gameTime);
         }
         else
         {
            GraphicsDevice.Clear(BaseScene.StandardBackgroundColor);
         }

         Performance.Pop();
      }

      public void PointerPressed(Vector2 scaledPosition, PointerType pointerType)
      {
         if (CurrentScene != null)
         {
            CurrentScene.PointerDown(scaledPosition, pointerType);
         }
      }

      public void PointerReleased(Vector2 scaledPosition, PointerType pointerType)
      {
         if (CurrentScene != null)
         {
            CurrentScene.PointerUp(scaledPosition, pointerType);
         }
      }

      public void PointerMoved(Vector2 scaledPosition)
      {
         if (CurrentScene != null)
         {
            CurrentScene.PointerMoved(scaledPosition);
         }
      }
   }
}
