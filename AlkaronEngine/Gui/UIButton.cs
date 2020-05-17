using AlkaronEngine.Assets.TextureFonts;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using System.Numerics;
using Veldrid;

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
        public RgbaFloat BorderColor { get; set; }
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
        public UIButton(string setText, TextureFont setFont, UIButtonStyle setButtonStyle = UIButtonStyle.None)
            : base(setText, setFont)
        {
            Padding = 5;
            ButtonStyle = setButtonStyle;
            isPressed = false;
            BorderColor = RgbaFloat.Black;
            BorderWidth = 1;
            Focusable = true;
        }
        #endregion

        #region Draw
        protected override void Draw(RenderContext renderContext)
        {
            base.Draw(renderContext);

            Vector2 screenPos = ScreenPosition;
            RectangleF rect = new RectangleF(screenPos.X, screenPos.Y, Width, Height);

            switch (ButtonStyle)
            {
                case UIButtonStyle.None:
                    {
                        if (isPressed)
                        {
                            RgbaFloat pressedColor = new RgbaFloat(new Vector4(Vector3.Zero, 0.3f * CompositeAlpha));
                            ScreenQuad.FillRectangle(renderContext, rect, pressedColor);
                        }
                    }
                    break;

                case UIButtonStyle.Custom:
                    // Nothing to do here
                    break;

                case UIButtonStyle.Flat:
                    {
                        if (isPressed)
                        {
                            RgbaFloat pressedColor = new RgbaFloat(new Vector4(Vector3.Zero, 0.3f * CompositeAlpha));
                            ScreenQuad.FillRectangle(renderContext, rect, pressedColor);
                        }

                        RgbaFloat borderColor = new RgbaFloat(new Vector4(BorderColor.R, BorderColor.G, BorderColor.B, CompositeAlpha));
                        ScreenQuad.RenderRectangle(renderContext, rect, borderColor, BorderWidth);
                    }
                    break;
            }
        }
        #endregion

        #region MouseDown/MouseUp
        protected internal override bool PointerDown(Vector2 position, PointerType pointerType, double gameTime)
        {
            if (base.PointerDown(position, pointerType, gameTime))
            {
                return true;
            }

            CaptureInput();
            isPressed = true;

            OnPointerDownInside?.Invoke(this, position, gameTime);

            return true;
        }

        protected internal override bool PointerUp(Vector2 position, PointerType pointerType, double gameTime)
        {
            if (base.PointerUp(position, pointerType, gameTime))
            {
                return true;
            }

            ReleaseInput();
            isPressed = false;

            if (HitTest(position) == true)
            {
                OnPointerUpInside?.Invoke(this, position, gameTime);
            }
            else
            {
                OnPointerUpOutside?.Invoke(this, position, gameTime);
            }

            return true;
        }
        #endregion

        #region KeyPressed/KeyReleased
        protected internal override bool KeyPressed(Key key, double gameTime)
        {
            if (base.KeyPressed(key, gameTime) == true)
            {
                return true;
            }

            if (key == Key.Enter)
            {
                isPressed = true;
            }

            return true;
        }

        protected internal override bool KeyReleased(Key key, double gameTime)
        {
            if (base.KeyReleased(key, gameTime) == true)
            {
                return true;
            }

            if (key == Key.Enter)
            {
                isPressed = false;
                OnPointerUpInside?.Invoke(this, Vector2.Zero, gameTime);
            }

            return true;
        }
        #endregion
    }
}
