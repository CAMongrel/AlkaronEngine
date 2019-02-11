using AlkaronEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading.Tasks;

namespace AlkaronViewer
{
    public class MainGame : AlkaronCoreGame
    {
        internal static MainGame Instance;

        public MainGame()
            : base(1280, 1024)
        {
            Instance = this;

            IsMouseVisible = true;
        }

        private void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            SceneManager?.ClientSizeChanged();
        }

        protected override void Initialize()
        {
            base.Initialize();

            MeshScene meshScene = new MeshScene();
            SceneManager.NextScene = meshScene;
        }
    }
}
