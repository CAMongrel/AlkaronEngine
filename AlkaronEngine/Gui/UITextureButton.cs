#region Using directives
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace AlkaronEngine.Gui
{
   public class UITextureButton : UIButton
   {
      #region Variables
      protected Graphics2D.Texture backgroundNotClicked;
      protected Graphics2D.Texture backgroundClicked;
      #endregion

      #region Constructor
      public UITextureButton (IRenderConfiguration renderConfig, string setText, SpriteFont setFont, 
         Graphics2D.Texture releaseButtonTexture, Graphics2D.Texture pressedButtonTexture)
         : base(renderConfig, setText, setFont)
      {
         backgroundClicked = pressedButtonTexture;
         backgroundNotClicked = releaseButtonTexture;

         Width = 0;
         Height = 0;
         if (backgroundNotClicked != null)
         {
            Width = backgroundNotClicked.Width;
            Height = backgroundNotClicked.Width;
         }
      }
      #endregion

      #region Render
      protected override void Draw()
      {
         Graphics2D.Texture background = null;
         if (isPressed)
         {
            background = backgroundClicked;
         }
         else
         {
            background = backgroundNotClicked;
         }

         if (background == null)
         {
            return;
         }

         Vector2 screenPos = ScreenPosition;

         Color col = Color.FromNonPremultiplied (new Vector4 (Vector3.One, CompositeAlpha));
         background.RenderOnScreen (screenPos.X, screenPos.Y, Width, Height, col);

         base.Draw();
      }
      #endregion
   }
}
