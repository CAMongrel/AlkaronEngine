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
        public MainGame()
            : base(1280, 1024)
        {
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
