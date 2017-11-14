using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AlkaronEngine.Scene;
using AlkaronEngine.Graphics2D;

namespace AlkaronEngine
{
   /// <summary>
   /// This is the main type for your game.
   /// </summary>
   public class AlkaronCoreGame : Game
   {
      internal static AlkaronCoreGame Core;

      protected GraphicsDeviceManager graphics;

      public SceneManager SceneManager { get; protected set; }

      public AlkaronCoreGame(int setPreferredBackbufferWidth = 1280,
                             int setPreferredBackbufferHeight = 720,
                             string setContentFolder = "Content")
      {
         Core = this;

         graphics = new GraphicsDeviceManager(this);
         graphics.PreferredBackBufferWidth = setPreferredBackbufferWidth;
         graphics.PreferredBackBufferHeight = setPreferredBackbufferHeight;
         graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
         graphics.ApplyChanges();

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

         SceneManager = new SceneManager(GraphicsDevice);
         ScreenQuad.Initialize(SceneManager);
         Graphics2D.Texture.SingleWhite = new Graphics2D.Texture(SceneManager, 1, 1, new byte[] { 255, 255, 255, 255 });
      }

      /// <summary>
      /// 
      /// </summary>
      protected override void OnExiting(object sender, EventArgs args)
      {
         base.OnExiting(sender, args);

         SceneManager?.Shutdown();
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
   }
}
