using AlkaronEngine.Graphics;
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

         UIWindowManager.AddWindow(this);
      }
      #endregion

      #region Close
      public void Close()
      {
         UIWindowManager.RemoveWindow(this);
      }

      public virtual void DidRemove()
      {
      }
      #endregion

      #region Render
      public override void Draw()
      {
         base.Draw();
      }
      #endregion

      #region Unit testing
      internal static void TestWindow()
      {
         throw new NotImplementedException();
         /*UIWindow wnd = null;

         TestGame.Start("TestWindow",
             delegate
             {
                 UIResource tr = new UIResource("Main Menu Text");
                 wnd = Window.FromUIResource(tr);
             },
             delegate
             {
                 wnd.Render();
             });*/
      }
      #endregion
   }
}
