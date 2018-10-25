using AlkaronEngine.Graphics2D;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AlkaronEngine.Scene
{
    public class SceneManager : IRenderConfiguration
    {
        private object lockObj = new object();

        public BaseScene CurrentScene { get; private set; }
        public BaseScene NextScene { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }
        public InputManager InputManager { get; private set; }
        public PrimitiveRenderManager RenderManager { get; private set; }

        public virtual Vector2 Scale
        {
            get
            {
                return new Vector2(1.0f, 1.0f);
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

            RenderManager = new PrimitiveRenderManager(this);
            InputManager = new InputManager(this);
            InputManager.OnPointerPressed += PointerPressed;
            InputManager.OnPointerReleased += PointerReleased;
            InputManager.OnPointerMoved += PointerMoved;
            InputManager.OnPointerWheelChanged += InputManager_OnPointerWheelChanged;
            InputManager.OnKeyPressed += InputManager_OnKeyPressed;
            InputManager.OnKeyReleased += InputManager_OnKeyReleased;

            CurrentScene = null;
            NextScene = null;
        }

        public void Shutdown()
        {
            lock (lockObj)
            {
                NextScene = null;

                CurrentScene?.Close();
                CurrentScene = null;
            }
        }

        public void ClientSizeChanged()
        {
            lock (lockObj)
            {
                CurrentScene?.ClientSizeChanged();

                ScreenQuad.RenderConfigDidUpdate();
            }
        }

        public void Update(GameTime gameTime)
        {
            Performance.Push("Game loop");

            InputManager.UpdateInput(gameTime);

            lock (lockObj)
            {
                if (NextScene != null)
                {
                    if (CurrentScene != null)
                    {
                        CurrentScene.Close();
                    }

                    CurrentScene = NextScene;
                    if (CurrentScene != null)
                    {
                        CurrentScene.Init(this);
                    }

                    NextScene = null;
                }

                if (CurrentScene != null)
                {
                    if (CurrentScene.MouseCursor != null)
                    {
                        CurrentScene.MouseCursor.Position = InputManager.MousePosition;
                    }

                    CurrentScene.Update(gameTime);
                }
            }

            Performance.Pop();
        }

        public void Draw(GameTime gameTime)
        {
            Performance.Push("Render loop on main thread");

            lock (lockObj)
            {
                if (CurrentScene != null)
                {
                    CurrentScene.Draw(gameTime);
                }
                else
                {
                    GraphicsDevice.Clear(BaseScene.StandardBackgroundColor);
                }
            }

            Performance.Pop();
        }

        public void PointerPressed(Vector2 scaledPosition, PointerType pointerType, GameTime gameTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerDown(scaledPosition, pointerType, gameTime);
            }
        }

        public void PointerReleased(Vector2 scaledPosition, PointerType pointerType, GameTime gameTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerUp(scaledPosition, pointerType, gameTime);
            }
        }

        public void PointerMoved(Vector2 scaledPosition, PointerType pointerType, GameTime gameTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerMoved(scaledPosition, gameTime);
            }
        }

        void InputManager_OnPointerWheelChanged(Vector2 position, Input.PointerType pointerType, GameTime gameTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerWheelChanged(position, gameTime);
            }
        }

        void InputManager_OnKeyPressed(Microsoft.Xna.Framework.Input.Keys key, GameTime gameTime)
        {
            lock (lockObj)
            {
                CurrentScene?.KeyPressed(key, gameTime);
            }
        }

        void InputManager_OnKeyReleased(Microsoft.Xna.Framework.Input.Keys key, GameTime gameTime)
        {
            lock (lockObj)
            {
                CurrentScene?.KeyReleased(key, gameTime);
            }
        }
    }
}
