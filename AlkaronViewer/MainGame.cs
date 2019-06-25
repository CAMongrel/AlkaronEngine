using AlkaronEngine;

namespace AlkaronViewer
{
    public class MainGame : AlkaronCoreGame
    {
        internal static MainGame Instance;

        public MainGame()
            : base(1280, 1024)
        {
            Instance = this;

            //IsMouseVisible = true;
        }

        private void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            //SceneManager?.ClientSizeChanged();
        }

        protected override void Initialize()
        {
            base.Initialize();

            /*MeshScene meshScene = new MeshScene();
            SceneManager.NextScene = meshScene;*/
        }
    }
}
