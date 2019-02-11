using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AlkaronEngine.Scene;
using AlkaronEngine.Graphics2D;
using System.Reflection;
using System.IO;
using AlkaronEngine.Assets;

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
    public class AlkaronCoreGame : Game
    {
        internal static AlkaronCoreGame Core;

        protected GraphicsDeviceManager graphics;

        public SceneManager SceneManager { get; protected set; }

        public SpriteFont DefaultFont { get; protected set; }

        public string ContentDirectory { get; protected set; }

        public PackageManager PackageManager { get; private set; }

        public AssetManager AssetManager { get; private set; }

        public GraphicsLibrary GraphicsLibrary
        {
            get
            {
                return GraphicsLibrary.OpenGL;            
            } 
        }

        public AlkaronCoreGame(int setPreferredBackbufferWidth = 1280,
                               int setPreferredBackbufferHeight = 720,
                               string setContentFolder = "Content")
        {
            Core = this;

            ContentDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "Content");

            if (Directory.Exists(ContentDirectory) == false)
            {
                Directory.CreateDirectory(ContentDirectory); 
            }

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = setPreferredBackbufferWidth;
            graphics.PreferredBackBufferHeight = setPreferredBackbufferHeight;
            graphics.ApplyChanges();

            // Replace ContentManager with custom implementation using
            // the ServiceProvider of the default ContentManager
            Content = new AlkaronContentManager(Content.ServiceProvider);
            Content.RootDirectory = setContentFolder;
        }

        protected void SetWindowSize(int width, int height)
        {
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            AssetManager = new AssetManager();

            PackageManager = new PackageManager();
            PackageManager.BuildPackageMap();

            SceneManager = new SceneManager(GraphicsDevice);

            ScreenQuad.Initialize(SceneManager);
            Graphics2D.Texture.SingleWhite = new Graphics2D.Texture(SceneManager, 1, 1, new byte[] { 255, 255, 255, 255 });

            DefaultFont = Content.Load<SpriteFont>("DefaultFont");
            SceneManager.PrimitiveRenderManager.EngineFont = DefaultFont;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullClockwiseFace;

            GraphicsDevice.RasterizerState = rasterizerState;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);

            SceneManager?.Shutdown();
            SceneManager = null;

            PackageManager?.Cleanup();
            PackageManager = null;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //TODO: use this.Content to load your game content here 
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SceneManager.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SceneManager.Draw(gameTime);
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
