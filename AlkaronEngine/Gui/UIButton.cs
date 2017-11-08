using System;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Gui;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Gui
{
   public enum UIButtonStyle
   {
      // No specific style, will only show the text and nothing else
      None,
      // Completely custom design. If Draw() is not overridden, this will only 
      // show the sub-components (if any) and the background color (if set).
      Custom,
      // Shows the text and a border. If the button is pressed, a simple pressed
      // effect is displayed.
      Flat,
   }

   public class UIButton : UILabel
   {
      #region Variables
      protected bool isPressed;
      #endregion

      #region Properties
      public UIButtonStyle ButtonStyle { get; set; }
      public Color BorderColor { get; set; }
      public int BorderWidth { get; set; }

      public int Padding { get; set; }

      public override Vector2 PreferredSize
      {
         get
         {
            Vector2 resultSize = base.PreferredSize;

            // Add padding
            resultSize.X += Padding * 2;
            resultSize.Y += Padding * 2;

            return resultSize;
         }
      }
      #endregion

      #region Events
      public event OnPointerDownInside OnPointerDownInside;
      public event OnPointerUpInside OnPointerUpInside;
      public event OnPointerUpOutside OnPointerUpOutside;
      #endregion

      #region ctor
      public UIButton(IRenderConfiguration setRenderConfig, string setText, SpriteFont setFont, UIButtonStyle setButtonStyle = UIButtonStyle.None)
         : base(setRenderConfig, setText, setFont, null)
      {
         Padding = 5;
         ButtonStyle = setButtonStyle;
         isPressed = false;
         BorderColor = Color.Black;
         BorderWidth = 1;
      }
      #endregion

      #region Draw
      protected override void Draw()
      {
         base.Draw();

         switch (ButtonStyle)
         {
            case UIButtonStyle.None:
               {
                  if (isPressed)
                  {
                     Color pressedColor = Color.FromNonPremultiplied(new Vector4(Vector3.Zero, 0.3f * CompositeAlpha));
                     Vector2 screenPos = ScreenPosition;
                     RectangleF rect = new RectangleF(screenPos.X, screenPos.Y, Width, Height);
                     Graphics2D.Texture.FillRectangle(renderConfig, rect, pressedColor);
                  }
               }
               break;

            case UIButtonStyle.Custom:
               // Nothing to do here
               break;

            case UIButtonStyle.Flat:
               {
                  Vector2 screenPos = ScreenPosition;
                  RectangleF rect = new RectangleF(screenPos.X, screenPos.Y, Width, Height);

                  if (isPressed)
                  {
                     Color pressedColor = Color.FromNonPremultiplied(new Vector4(Vector3.Zero, 0.3f * CompositeAlpha));
                     Graphics2D.Texture.FillRectangle(renderConfig, rect, pressedColor);
                  }

                  Color borderColor = Color.FromNonPremultiplied(new Vector4(BorderColor.ToVector3(), CompositeAlpha));
                  Graphics2D.Texture.RenderRectangle(renderConfig, rect, borderColor, BorderWidth);
               }
               break;
         }
      }
      #endregion

      #region MouseDown
      protected internal override bool PointerDown(Vector2 position, PointerType pointerType)
      {
         if (!base.PointerDown(position, pointerType))
         {
            return false;
         }

         CaptureInput();
         isPressed = true;

         OnPointerDownInside?.Invoke(this, position);

         return true;
      }
      #endregion

      #region MouseUp
      protected internal override bool PointerUp(Vector2 position, PointerType pointerType)
      {
         if (!base.PointerUp(position, pointerType))
         {
            return false;
         }

         ReleaseInput();
         isPressed = false;

         if (HitTest(position) == true)
         {
            OnPointerUpInside?.Invoke(this, position);
         } else
         {
            OnPointerUpOutside?.Invoke(this, position);
         }

         return true;
      }
      #endregion
   }
}
