using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using AlkaronEngine.Actors;
using AlkaronEngine.Components;
using AlkaronEngine.Controllers;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Gui;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using Veldrid;

namespace AlkaronEngine.Scene
{
    public class BaseScene
    {
        public static RgbaFloat DefaultClearColor = new RgbaFloat(0.5f, 0.0f, 0.0f, 1.0f);
        public RgbaFloat ClearColor = DefaultClearColor;

        /*public Gui.MouseCursor MouseCursor { get; set; }

        public IRenderConfiguration RenderConfig { get; private set; }

        public ContentManager ContentManager { get; private set; }*/

        public SceneGraph SceneGraph { get; private set; }

        public RenderManager RenderManager { get; private set; }

        public CameraActor CurrentCamera { get; set; }

        public BaseController CurrentController { get; private set; }

        private bool isMouseCaptured;
        private Vector2 lastMousePos;
        private bool mouseCursorWasVisible;

        public Vector2 ScreenSize => new Vector2(AlkaronCoreGame.Core.Window.Width, AlkaronCoreGame.Core.Window.Height);

        public BaseScene()
        {
            CurrentController = null;
        }

        /*public virtual Color BackgroundColor
        {
            get
            {
                return StandardBackgroundColor;
            }
        }*/

        internal virtual void Init()
        {
            mouseCursorWasVisible = false;
            isMouseCaptured = false;

            //GuiCore = new GuiCore();
            //GuiCore.Initialize((int)ScreenSize.X, (int)ScreenSize.Y);

            /*ContentManager = new ContentManager(AlkaronCoreGame.Core.Content.ServiceProvider, "Content");*/
            SceneGraph = new SceneGraph(this);
            RenderManager = new RenderManager();

            Init3D();
            InitUI();

            RenderManager.Start();
        }

        internal void ClientSizeChanged()
        {
            RenderManager.SizeChanged();
            //GuiCore.Resize((int)ScreenSize.X, (int)ScreenSize.Y);
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
            CurrentCamera = new CameraActor(new FlyCameraComponent(new Vector3(0, 0, -15),
                                                     ScreenSize,
                                                     0.1f,
                                                     500.0f));

            SceneGraph.AddActor(CurrentCamera);
        }

        public virtual void Close()
        {
            RenderManager.Stop();

            UIWindowManager.Clear();

            /*ContentManager.Unload();
            ContentManager.Dispose();
            ContentManager = null;*/
        }

        public void UpdateInput(InputSnapshot snapshot, double deltaTime)
        {
            // Update 2D UI with input changes
            //GuiCore.Update(deltaTime, snapshot);
        }

        public virtual void Update(double deltaTime)
        {
            // Update 3D graph
            RenderManager.SetWorldOriginForDepthSorting(CurrentCamera.CameraComponent);
            SceneGraph.Update(deltaTime);

            // Update 2D UI
            UIWindowManager.Update(deltaTime);
        }

        public virtual void Draw(double deltaTime, RenderContext renderContext)
        {
            renderContext.RenderManager = RenderManager;

            Performance.Enabled = true;
            Performance.LogLongRunningTasksOnly = true;

            renderContext.RenderedTrianglesThisFrame = 0;
            renderContext.CommandList.ClearColorTarget(0, ClearColor);

            RenderManager.SetViewTargetFromCameraComponent(CurrentCamera.CameraComponent);

            if (RenderManager.SupportsMultiThreadedRendering == false)
            {
                RenderManager.RenderFrame(renderContext);
            }

            //GuiCore.RenderOnScreen(renderContext);
        }

        public virtual void PointerDown(Vector2 position, PointerType pointerType, double deltaTime)
        {
            bool res = UIWindowManager.PointerDown(position, pointerType, deltaTime);
            if (res == false)
            {
                // Event was not handled by UI
                mouseCursorWasVisible = AlkaronCoreGame.Core.Window.CursorVisible;
                AlkaronCoreGame.Core.Window.CursorVisible = false;
                isMouseCaptured = true;

                lastMousePos = position;

                res = CurrentController?.PointerDown(position, pointerType) ?? false;
                if (res == false)
                {
                    CurrentCamera?.PointerDown(position, pointerType);
                }
            }
        }

        public virtual void PointerUp(Vector2 position, PointerType pointerType, double deltaTime)
        {
            bool res = UIWindowManager.PointerUp(position, pointerType, deltaTime);
            if (res == false)
            {
                // Event was not handled by UI
                AlkaronCoreGame.Core.Window.CursorVisible = mouseCursorWasVisible;
                isMouseCaptured = false;

                res = CurrentController?.PointerUp(position, pointerType) ?? false;
                if (res == false)
                {
                    CurrentCamera?.PointerUp(position, pointerType);
                }
            }
        }

        public virtual void PointerMoved(Vector2 position, double deltaTime)
        {
            bool res = UIWindowManager.PointerMoved(position, deltaTime);
            if (res == false)
            {
                // Event was not handled by UI
                if (isMouseCaptured)
                {
                    res = CurrentController?.PointerMoved(position) ?? false;

                    if (res == false)
                    {
                        CurrentCamera?.PointerMoved(position, deltaTime);
                    }

                    AlkaronCoreGame.Core.Window.SetMousePosition((int)lastMousePos.X, (int)lastMousePos.Y);
                }
            }
        }

        public virtual void PointerWheelChanged(Vector2 deltaValue, double deltaTime)
        {
            bool res = UIWindowManager.PointerWheelChanged(deltaValue, deltaTime);
            if (res == false)
            {
                // Event was not handled by UI
                res = CurrentController?.PointerWheelChanged(deltaValue) ?? false;
                if (res == false)
                {
                    CurrentCamera?.PointerWheelChanged(deltaValue, deltaTime);
                }
            }
        }

        public virtual bool KeyPressed(Key key, double deltaTime)
        {
            bool res = UIWindowManager.KeyPressed(key, deltaTime);
            if (res == false)
            {
                // Event was not handled by UI
                res = CurrentController?.KeyPressed(key) ?? false;
                if (res == false)
                {
                    CurrentCamera?.KeyPressed(key);
                }
            }
            return res;
        }

        public virtual bool KeyReleased(Key key, double deltaTime)
        {
            bool res = UIWindowManager.KeyReleased(key, deltaTime);
            if (res == false)
            {
                // Event was not handled by UI
                res = CurrentController?.KeyReleased(key) ?? false;
                if (res == false)
                {
                    CurrentCamera?.KeyReleased(key);
                }
            }
            return res;
        }
    }
}
