using System;
using System.Diagnostics.Contracts;
using AlkaronEngine.Actors;
using AlkaronEngine.Components;
using AlkaronEngine.Controllers;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Gui;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AlkaronEngine.Scene
{
    public class BaseScene
    {
        public static Color StandardBackgroundColor = new Color(0x7F, 0x00, 0x00);

        public Gui.MouseCursor MouseCursor { get; set; }

        public IRenderConfiguration RenderConfig { get; private set; }

        public ContentManager ContentManager { get; private set; }

        public SceneGraph SceneGraph { get; private set; }

        public RenderManager RenderManager { get; private set; }

        public CameraActor CurrentCamera { get; set; }

        public BaseController CurrentController { get; private set; }

        private bool isMouseCaptured;
        private Vector2 lastMousePos;
        private bool mouseCursorWasVisible;

        public BaseScene()
        {
            CurrentController = null;
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

            mouseCursorWasVisible = false;
            isMouseCaptured = false;

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
            CreateDefaultCamera();
        }

        protected virtual void CreateDefaultCamera()
        {
            CurrentCamera = new CameraActor();

            BaseComponent CurrentCameraComponent = new FlyCameraComponent(new Vector3(0, 0, 15),
                                                     RenderConfig.ScreenSize,
                                                     0.1f,
                                                     500.0f);

            CurrentCamera.AttachedComponents.Add(CurrentCameraComponent);

            SceneGraph.AddActor(CurrentCamera);
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
            long start = System.Diagnostics.Stopwatch.GetTimestamp();

            Performance.Push("BaseScene.Draw");

            RenderConfig.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Render 3D
            Performance.Push("BaseScene.Draw (SceneGraph)");
            RenderManager.UpdateMatricesFromCameraActor(CurrentCamera);
            SceneGraph.Draw(gameTime, RenderManager);       // SceneGraph.Draw() only creates the RenderProxies
            Performance.Pop();
            Performance.Push("BaseScene.Draw (Render3D)");
            // Clear only the depth and stencil buffer
            Performance.Push("BaseScene.Draw (Render3D - Initial clear)");
            Clear(BackgroundColor, ClearOptions.DepthBuffer | ClearOptions.Stencil, 1, 0);
            Performance.Pop();
            RenderManager.Draw(gameTime);
            Performance.Pop();

            // Render 2D
            Performance.Push("BaseScene.Draw (Render2D)");
            // Clear depth buffer and stencil again for 2D rendering
            Clear(BackgroundColor, ClearOptions.DepthBuffer | ClearOptions.Stencil, 1, 0);
            UIWindowManager.Draw(gameTime);
            MouseCursor?.Render(gameTime);
            Performance.Pop();

            Performance.Pop();

            long end = System.Diagnostics.Stopwatch.GetTimestamp();
            double deltaSeconds = (double)(end - start) / (double)System.Diagnostics.Stopwatch.Frequency;

            Console.WriteLine("Draw time " + deltaSeconds + " seconds (Avg FPS: " + (int)(1.0 / deltaSeconds) + ")");
        }

        public virtual void PointerDown(Vector2 position, PointerType pointerType)
        {
            bool res = UIWindowManager.PointerDown(position, pointerType);
            if (res == false)
            {
                // Event was not handled by UI
                if (MouseCursor != null)
                {
                    mouseCursorWasVisible = MouseCursor.IsVisible;
                    MouseCursor.IsVisible = false;
                }
                isMouseCaptured = true;

                lastMousePos = position;

                res = CurrentController?.PointerDown(position, pointerType) ?? false;
                if (res == false)
                {
                    CurrentCamera?.PointerDown(position, pointerType);
                }
            }
        }

        public virtual void PointerUp(Vector2 position, PointerType pointerType)
        {
            bool res = UIWindowManager.PointerUp(position, pointerType);
            if (res == false)
            {
                // Event was not handled by UI
                if (MouseCursor != null)
                {
                    MouseCursor.IsVisible = mouseCursorWasVisible;
                }
                isMouseCaptured = false;

                res = CurrentController?.PointerUp(position, pointerType) ?? false;
                if (res == false)
                {
                    CurrentCamera?.PointerUp(position, pointerType);
                }
            }
        }

        public virtual void PointerMoved(Vector2 position)
        {
            bool res = UIWindowManager.PointerMoved(position);
            if (res == false)
            {
                // Event was not handled by UI
                if (isMouseCaptured)
                {
                    res = CurrentController?.PointerMoved(position) ?? false;

                    if (res == false)
                    {
                        CurrentCamera?.PointerMoved(position);
                    }

                    Mouse.SetPosition((int)lastMousePos.X, (int)lastMousePos.Y);
                }
            }
        }

        public virtual void PointerWheelChanged(Vector2 position)
        {
            bool res = UIWindowManager.PointerWheelChanged(position);
            if (res == false)
            {
                // Event was not handled by UI
                res = CurrentController?.PointerWheelChanged(position) ?? false;
                if (res == false)
                {
                    CurrentCamera?.PointerWheelChanged(position);
                }
            }
        }

        public virtual bool OnKeyEvent(Keys key, KeyEventType eventType)
        {
            bool res = UIWindowManager.KeyEvent(key, eventType);
            if (res == false)
            {
                // Event was not handled by UI
                res = CurrentController?.OnKeyEvent(key, eventType) ?? false;
                if (res == false)
                {
                    CurrentCamera?.OnKeyEvent(key, eventType);
                }
            }
            return res;
        }
    }
}
