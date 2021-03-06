﻿using System;
using System.Diagnostics.Contracts;
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
            SceneGraph = new SceneGraph(this);
            RenderManager = new RenderManager(RenderConfig);

            Init3D();
            InitUI();

            RenderManager.Start();
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
            CurrentCamera = new CameraActor(new FlyCameraComponent(new Vector3(0, 0, 15),
                                                     RenderConfig.ScreenSize,
                                                     0.1f,
                                                     500.0f));

            SceneGraph.AddActor(CurrentCamera);
        }

        public virtual void Close()
        {
            RenderManager.Stop();

            UIWindowManager.Clear();

            ContentManager.Unload();
            ContentManager.Dispose();
            ContentManager = null;
        }

        public virtual void Update(GameTime gameTime)
        {
            // Update 3D graph
            SceneGraph.Update(gameTime);

            // Update 2D UI
            UIWindowManager.Update(gameTime);
        }

        public virtual void Draw(GameTime gameTime)
        {
            RenderManager.SetViewTargetFromCameraComponent(CurrentCamera.CameraComponent);
            RenderManager.MouseCursor = MouseCursor;

            if (RenderManager.SupportsMultiThreadedRendering == false)
            {
                RenderManager.RenderFrame();
            }
        }

        public virtual void PointerDown(Vector2 position, PointerType pointerType, GameTime gameTime)
        {
            bool res = UIWindowManager.PointerDown(position, pointerType, gameTime);
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

        public virtual void PointerUp(Vector2 position, PointerType pointerType, GameTime gameTime)
        {
            bool res = UIWindowManager.PointerUp(position, pointerType, gameTime);
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

        public virtual void PointerMoved(Vector2 position, GameTime gameTime)
        {
            bool res = UIWindowManager.PointerMoved(position, gameTime);
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

        public virtual void PointerWheelChanged(Vector2 deltaValue, GameTime gameTime)
        {
            bool res = UIWindowManager.PointerWheelChanged(deltaValue, gameTime);
            if (res == false)
            {
                // Event was not handled by UI
                res = CurrentController?.PointerWheelChanged(deltaValue) ?? false;
                if (res == false)
                {
                    CurrentCamera?.PointerWheelChanged(deltaValue);
                }
            }
        }

        public virtual bool KeyPressed(Keys key, GameTime gameTime)
        {
            bool res = UIWindowManager.KeyPressed(key, gameTime);
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

        public virtual bool KeyReleased(Keys key, GameTime gameTime)
        {
            bool res = UIWindowManager.KeyReleased(key, gameTime);
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
