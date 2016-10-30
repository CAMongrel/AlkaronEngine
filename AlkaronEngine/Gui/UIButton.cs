#region Using directives
using AlkaronEngine.Graphics;
using AlkaronEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace AlkaronEngine.Gui
{
   internal class UIButton : UILabel
   {
      #region Variables
      private Graphics.Texture backgroundNotClicked;
      private Graphics.Texture backgroundClicked;
      protected bool isPressed;
      #endregion

      #region Events
      public event OnPointerDownInside OnMouseDownInside;
      public event OnPointerUpInside OnMouseUpInside;
      #endregion

      #region Constructor
      public UIButton (IRenderConfiguration renderConfig, string setText, SpriteFont setFont, 
         Graphics.Texture releaseButtonTexture, Graphics.Texture pressedButtonTexture)
         : base(renderConfig, setText, setFont)
      {
         if (releaseButtonTexture == null)
         {
            throw new ArgumentNullException(nameof(releaseButtonTexture));
         }
         if (pressedButtonTexture == null)
         {
            throw new ArgumentNullException(nameof(pressedButtonTexture));
         }

         isPressed = false;

         backgroundClicked = pressedButtonTexture;
         backgroundNotClicked = releaseButtonTexture;

         Width = backgroundNotClicked.Width;
         Height = backgroundNotClicked.Width;
      }
      #endregion

      #region Render
      public override void Draw()
      {
         Graphics.Texture background = null;
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

      #region MouseDown
      public override bool PointerDown (Vector2 position, PointerType pointerType)
      {
         if (!base.PointerDown (position, pointerType))
            return false;

         isPressed = true;

         OnMouseDownInside?.Invoke(position);

         return true;
      }
      #endregion

      #region MouseUp
      public override bool PointerUp (Vector2 position, PointerType pointerType)
      {
         if (!base.PointerUp (position, pointerType))
            return false;

         isPressed = false;

         OnMouseUpInside?.Invoke(position);

         return true;
      }
      #endregion
   }
}
