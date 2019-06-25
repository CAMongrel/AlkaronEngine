using System;
using System.Reflection;
using System.IO;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid;
using System.Diagnostics;
using AlkaronEngine.Scene;

namespace AlkaronEngine
{
    public enum GraphicsLibrary
    {
        DirectX11,
        OpenGL
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AlkaronCoreGame : IDisposable
    {
        private readonly Sdl2Window Window;
        public GraphicsDevice GraphicsDevice { get; private set; }

        internal static AlkaronCoreGame Core;

        public SceneManager SceneManager { get; protected set; }

        /*public SpriteFont DefaultFont { get; protected set; }*/

        public string ContentDirectory { get; protected set; }

        /*public PackageManager PackageManager { get; private set; }

        public AssetManager AssetManager { get; private set; }

        public AlkaronContentManager AlkaronContent { get; private set; }*/

        public GraphicsLibrary GraphicsLibrary
        {
            get
            {
                return GraphicsLibrary.OpenGL;            
            } 
        }

        public AlkaronCoreGame(int setWidth = 1280, int setHeight = 720,
                               string setContentFolder = "Content",
                               string setWindowTitle = "AlkaronEngine")
        {
            Core = this;

            ContentDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "Content");

            if (Directory.Exists(ContentDirectory) == false)
            {
                Directory.CreateDirectory(ContentDirectory); 
            }

            WindowCreateInfo wci = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowTitle = setWindowTitle,
            };
            Window = VeldridStartup.CreateWindow(ref wci);

            // Replace ContentManager with custom implementation using
            // the ServiceProvider of the default ContentManager
            /*AlkaronContent = new AlkaronContentManager(Content.ServiceProvider);
            Content = AlkaronContent;
            Content.RootDirectory = setContentFolder;*/
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected virtual void Initialize()
        {
            SceneManager = new SceneManager();

            /*AssetManager = new AssetManager();

            PackageManager = new PackageManager();
            // "AlkaronContent" must be initialized before calling this, because
            // BuildPackageMap depends on it.
            PackageManager.BuildPackageMap();

            ScreenQuad.Initialize(SceneManager);
            Graphics2D.Texture.SingleWhite = new Graphics2D.Texture(SceneManager, 1, 1, new byte[] { 255, 255, 255, 255 });

            DefaultFont = Content.Load<SpriteFont>("DefaultFont");
            SceneManager.PrimitiveRenderManager.EngineFont = DefaultFont;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullClockwiseFace;

            GraphicsDevice.RasterizerState = rasterizerState;*/
        }

        public void Dispose()
        {
            OnExiting();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnExiting()
        {
            SceneManager?.Shutdown();
            SceneManager = null;

            /*PackageManager?.Cleanup();
            PackageManager = null;*/
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected virtual void LoadContent()
        {
            //TODO: use this.Content to load your game content here 
        }

        /// <summary>
        /// Main entry method for running the application.
        /// Blocks until exit.
        /// </summary>
        public void Run()
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true);
#if DEBUG
            options.Debug = true;
#endif
            GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, options);
            //factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            //GraphicsDeviceCreated?.Invoke(gd, factory, gd.MainSwapchain);

            Stopwatch sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            while (Window.Exists)
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                float deltaSeconds = (float)(newElapsed - previousElapsed);

                InputSnapshot inputSnapshot = Window.PumpEvents();
                SceneManager.UpdateFrameInput(inputSnapshot);

                if (Window.Exists)
                {
                    previousElapsed = newElapsed;
                    /*if (_windowResized)
                    {
                        _windowResized = false;
                        _gd.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
                        Resized?.Invoke();
                    }*/

                    Update(deltaSeconds);
                    Draw(deltaSeconds);
                }
            }

            GraphicsDevice.WaitForIdle();
            //factory.DisposeCollector.DisposeAll();
            GraphicsDevice.Dispose();
            //GraphicsDeviceDestroyed?.Invoke();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected virtual void Update(double deltaTime)
        {
            SceneManager.Update(deltaTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected virtual void Draw(double deltaTime)
        {
            SceneManager.Draw(deltaTime);
        }

        /// <summary>
        /// Passes log output to the log handler
        /// </summary>
        internal void Log(string text)
        {
            Console.WriteLine(text);
        }
    }
}
