using System;
using System.Reflection;
using System.IO;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid;
using System.Diagnostics;
using AlkaronEngine.Scene;
using AlkaronEngine.Assets;
using AlkaronEngine.Gui;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Assets.TextureFonts;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Util;

namespace AlkaronEngine
{
    public class EngineConfiguration
    {
        public int WindowWidth { get; set; } = 1280;
        public int WindowHeight { get; set; } = 1024;
        public int X { get; set; } = 100;
        public int Y { get; set; } = 100;
        public WindowState WindowState { get; set; } = WindowState.BorderlessFullScreen;
        public string WindowTitle { get; set; } = "AlkaronEngine";
        public string ContentFolder { get; set; } = "Content";
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AlkaronCoreGame : IDisposable, ITimeProvider, ILogWriter
    {
        internal readonly Sdl2Window Window;
        public GraphicsDevice? GraphicsDevice { get; private set; }

        internal static AlkaronCoreGame? Core;

        public SceneManager? SceneManager { get; protected set; }

        public TextureFont? DefaultFont { get; protected set; }

        public string ContentDirectory { get; protected set; }

        public PackageManager? PackageManager { get; private set; }

        public AssetManager? AssetManager { get; private set; }

        public AlkaronContentManager AlkaronContent { get; private set; }

        internal ShaderManager? ShaderManager { get; private set; }

        internal EngineConfiguration EngineConfiguration { get; private set; }

        public AlkaronCoreGame(EngineConfiguration configuration)
        {
            Core = this;

            EngineConfiguration = configuration;

            Performance.TimeProvider = this;
            Log.LogWriter = this;

            ContentDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                EngineConfiguration.ContentFolder);

            if (Directory.Exists(ContentDirectory) == false)
            {
                Directory.CreateDirectory(ContentDirectory); 
            }

            WindowCreateInfo wci = new WindowCreateInfo
            {
                X = EngineConfiguration.X,
                Y = EngineConfiguration.Y,
                WindowWidth = EngineConfiguration.WindowWidth,
                WindowHeight = EngineConfiguration.WindowHeight,
                WindowTitle = EngineConfiguration.WindowTitle,
                WindowInitialState = EngineConfiguration.WindowState
            };
            Window = VeldridStartup.CreateWindow(ref wci);

            AlkaronContent = new AlkaronContentManager();
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

            AssetManager = new AssetManager();
            AssetManager.AssetSettings.GraphicsDevice = GraphicsDevice;

            PackageManager = new PackageManager();
            // "AlkaronContent" must be initialized before calling this, because
            // BuildPackageMap depends on it.
            PackageManager.BuildPackageMap();

            ShaderManager = new ShaderManager();
            ShaderManager.Initialize();

            TextRenderer.Initialize(GraphicsDevice.ResourceFactory);
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Shutdown()
        {
            SceneManager?.Shutdown();
            SceneManager = null;

            PackageManager?.Cleanup();
            PackageManager = null;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected virtual void LoadContent()
        {
            DefaultFont = AssetManager.Load<TextureFont>("EngineDefaults", "DefaultFont.textureFont");

            /*AssetManager.AssetSettings.ReadOnlyAssets = false;

            Package pkg = PackageManager.CreatePackage("EngineDefaults", "EngineDefaults", AssetManager.AssetSettings);
            Assets.Importers.AssetImporterSurface2D.Import(@"C:\Users\Henning\Downloads\Consolas20.png", "DefaultFontSurface", "EngineDefaults",
                AssetManager.AssetSettings, out Assets.Materials.Surface2D? surface2D);
            Assets.Importers.AssetImporterTextureFont.Import(surface2D, @"C:\Users\Henning\Downloads\Consolas20.json", "DefaultFont", "EngineDefaults",
                AssetManager.AssetSettings, out Assets.TextureFonts.TextureFont? textureFont);
            pkg.Save(AssetManager.AssetSettings);*/
        }

        /// <summary>
        /// Main entry method for running the application.
        /// Blocks until exit.
        /// </summary>
        public void Run()
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R32_Float,
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

            Initialize();
            LoadContent();

            Stopwatch sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            while (Window.Exists)
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                float deltaSeconds = (float)(newElapsed - previousElapsed);

                InputSnapshot inputSnapshot = Window.PumpEvents();
                SceneManager.UpdateFrameInput(inputSnapshot, deltaSeconds);

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

            Shutdown();

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
            SceneManager.Draw(GraphicsDevice, deltaTime);
        }

        public long GetTimestamp()
        {
            return Stopwatch.GetTimestamp();
        }

        public long GetFrequency()
        {
            return Stopwatch.Frequency;
        }

        public void WriteLine(string line, params object[] args)
        {
            Console.WriteLine(line, args);
        }
    }
}
