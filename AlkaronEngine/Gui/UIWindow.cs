using AlkaronEngine.Graphics3D;

namespace AlkaronEngine.Gui
{
    public class UIWindow : UIBaseComponent
    {
        #region Constructor
        public UIWindow()
        {
            X = Y = 0;
            Width = AlkaronCoreGame.Core.Window.Width;
            Height = AlkaronCoreGame.Core.Window.Height;

            WidthSizeMode = UISizeMode.Fit;
            HeightSizeMode = UISizeMode.Fit;
        }
        #endregion

        #region Show
        public void Show()
        {
            if (UIWindowManager.AddWindow(this))
            {
                DidShow();
            }
        }

        protected virtual void DidShow()
        {

        }
        #endregion

        #region Close
        public void Close()
        {
            ClearComponents();

            if (UIWindowManager.RemoveWindow(this))
            {
                DidRemove();
            }
        }

        protected virtual void DidRemove()
        {
        }
        #endregion

        #region Render
        protected override void Draw(RenderContext renderContext)
        {
            base.Draw(renderContext);
        }
        #endregion
    }
}
