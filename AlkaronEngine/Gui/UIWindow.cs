using AlkaronEngine.Graphics2D;
using System;

namespace AlkaronEngine.Gui
{
    public class UIWindow : UIBaseComponent
    {
        #region Constructor
        public UIWindow(IRenderConfiguration renderConfig)
           : base(renderConfig)
        {
            X = Y = 0;
            Width = renderConfig.ScreenSize.X;
            Height = renderConfig.ScreenSize.Y;

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
        protected override void Draw()
        {
            base.Draw();
        }
        #endregion
    }
}
