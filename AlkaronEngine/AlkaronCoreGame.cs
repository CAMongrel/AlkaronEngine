using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AlkaronEngine.Scene;
using AlkaronEngine.Graphics;

namespace AlkaronEngine
{
   /// <summary>
   /// This is the main type for your game.
   /// </summary>
   public class AlkaronCoreGame : Game
   {
      protected GraphicsDeviceManager graphics;

      public SceneManager SceneManager { get; protected set; }

      public AlkaronCoreGame(int setPreferredBackbufferWidth = 1280,
                             int setPreferredBackbufferHeight = 720,
                             string setContentFolder = "Content")
      {
         graphics = new GraphicsDeviceManager(this);
         graphics.PreferredBackBufferWidth = setPreferredBackbufferWidth;
         graphics.PreferredBackBufferHeight = setPreferredBackbufferHeight;
         graphics.ApplyChanges();

         Content.RootDirectory = setContentFolder;
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
         Graphics.Texture.SingleWhite = new Graphics.Texture(SceneManager, 1, 1, new byte[] { 255, 255, 255, 255 });
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
