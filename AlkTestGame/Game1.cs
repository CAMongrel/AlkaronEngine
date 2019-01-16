using System;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AlkTestGame
{
   public class Game1 : AlkaronEngine.AlkaronCoreGame, ILogWriter
   {
      public static Game1 MainGame;

      public Game1()
      {
         Log.LogWriter = this;

         MainGame = this;
      }

      public void WriteLine(string line, params object[] args)
      {
         Console.WriteLine(line, args);
      }

      protected override void Initialize()
      {
         DefaultFont = Content.Load<SpriteFont>("DefaultFont");

         base.Initialize();

         TestScene scene = new TestScene();
         SceneManager.NextScene = scene;
      }
   }
}
