using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Gui;
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

        public GraphicsDevice GraphicsDevice => AlkaronCoreGame.Core.GraphicsDevice;
        /*public PrimitiveRenderManager PrimitiveRenderManager { get; private set; }*/

        private CommandList commandList;
        private Pipeline pipeline;

        private RenderContext renderContext;


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

            CreateResources();
        }

        private void CreateResources()
        {
            var factory = GraphicsDevice.ResourceFactory;

            // Create pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription();
            pipelineDescription.ShaderSet.Shaders = new Shader[0];
            pipelineDescription.ShaderSet.Specializations = new SpecializationConstant[0];
            pipelineDescription.ShaderSet.VertexLayouts = new VertexLayoutDescription[0];
            pipelineDescription.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;

            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            commandList = factory.CreateCommandList();

            renderContext = new RenderContext();
            renderContext.CommandList = commandList;

            ScreenQuad.Initialize(factory);
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

        public void UpdateFrameInput(InputSnapshot snapshot, double deltaTime)
        {
            InputManager.UpdateInput(snapshot, deltaTime);
            CurrentScene?.UpdateInput(snapshot, deltaTime);
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
                    CurrentScene.Update(deltaTime);
                }
            }

            Performance.Pop();
        }

        public void Draw(GraphicsDevice graphicsDevice, double deltaTime)
        {
            Performance.Push("Render loop on main thread");

            renderContext.GraphicsDevice = graphicsDevice;

            commandList.Begin();

            // We want to render directly to the output window.
            commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            commandList.SetPipeline(pipeline);

            lock (lockObj)
            {
                if (CurrentScene != null)
                {
                    CurrentScene.Draw(deltaTime, renderContext);
                }
                else
                {
                    commandList.ClearColorTarget(0, BaseScene.DefaultClearColor);
                }
            }

            commandList.End();
            GraphicsDevice.SubmitCommands(commandList);

            GraphicsDevice.SwapBuffers();

            Performance.Pop();
        }

        public void PointerPressed(Vector2 scaledPosition, PointerType pointerType, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerDown(scaledPosition, pointerType, deltaTime);
            }
        }

        public void PointerReleased(Vector2 scaledPosition, PointerType pointerType, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerUp(scaledPosition, pointerType, deltaTime);
            }
        }

        public void PointerMoved(Vector2 scaledPosition, PointerType pointerType, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerMoved(scaledPosition, deltaTime);
            }
        }

        void InputManager_OnPointerWheelChanged(Vector2 position, Input.PointerType pointerType, double deltaTime)
        {
            lock (lockObj)
            {
                CurrentScene?.PointerWheelChanged(position, deltaTime);
            }
        }

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
