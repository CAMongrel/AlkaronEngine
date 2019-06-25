using AlkaronEngine.Input;
using AlkaronEngine.Util;
using System;
using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Scene
{
    public class SceneManager// : IRenderConfiguration
    {
        private object lockObj = new object();

        public BaseScene CurrentScene { get; private set; }
        public BaseScene NextScene { get; set; }

        public InputManager InputManager { get; private set; }

        /*public GraphicsDevice GraphicsDevice { get; private set; }
        public PrimitiveRenderManager PrimitiveRenderManager { get; private set; }*/

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

        /*public Vector2 ScreenSize
        {
            get
            {
                return new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            }
        }*/

        public SceneManager()//GraphicsDevice setGraphicsDevice)
        {
            /*if (setGraphicsDevice == null)
            {
                throw new ArgumentNullException(nameof(setGraphicsDevice));
            }
            GraphicsDevice = setGraphicsDevice;

            PrimitiveRenderManager = new PrimitiveRenderManager(this);*/
            InputManager = new InputManager();
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

                //ScreenQuad.RenderConfigDidUpdate();
            }
        }

        public void UpdateFrameInput(InputSnapshot snapshot, double gameTime)
        {
            InputManager.UpdateInput(snapshot, gameTime);
        }

        public void Update(double deltaTime)
        {
            Performance.Push("Game loop");

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
                        CurrentScene.Init();
                    }

                    NextScene = null;
                }

                if (CurrentScene != null)
                {
                    /*if (CurrentScene.MouseCursor != null)
                    {
                        CurrentScene.MouseCursor.Position = InputManager.MousePosition;
                    }*/

                    CurrentScene.Update(deltaTime);
                }
            }

            Performance.Pop();
        }

        public void Draw(double deltaTime)
        {
            Performance.Push("Render loop on main thread");

            lock (lockObj)
            {
                if (CurrentScene != null)
                {
                    CurrentScene.Draw(deltaTime);
                }
                else
                {
                    //GraphicsDevice.Clear(BaseScene.StandardBackgroundColor);
                }
            }

            Performance.Pop();
        }

        /*public void PointerPressed(Vector2 scaledPosition, PointerType pointerType, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerDown(scaledPosition, pointerType, gameTime);
            }
        }

        public void PointerReleased(Vector2 scaledPosition, PointerType pointerType, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerUp(scaledPosition, pointerType, gameTime);
            }
        }

        public void PointerMoved(Vector2 scaledPosition, PointerType pointerType, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerMoved(scaledPosition, gameTime);
            }
        }

        void InputManager_OnPointerWheelChanged(Vector2 position, Input.PointerType pointerType, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerWheelChanged(position, gameTime);
            }
        }*/

        void InputManager_OnKeyPressed(Key key, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.KeyPressed(key, deltaTime);
            }
        }

        void InputManager_OnKeyReleased(Key key, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.KeyReleased(key, deltaTime);
            }
        }
    }
}
