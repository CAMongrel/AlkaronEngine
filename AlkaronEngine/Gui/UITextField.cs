using AlkaronEngine.Assets.TextureFonts;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Gui
{
    public class UITextField : UIBaseComponent
    {
        private double blinkTimeStartTotalSeconds = 0.0;

        public TextureFont Font { get; set; }

        public RgbaFloat BorderColor { get; set; } = RgbaFloat.Black;
        public int BorderWidth { get; set; } = 1;

        public int Padding { get; set; } = 0;

        public string ShadowText { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        private int cursorPos;
        private bool renderCursor;
        private bool firstKeyRepeat;
        private Key lastPressedKey;
        private double lastPressedKeyTimestamp;

        public override Vector2 PreferredSize
        {
            get
            {
                Vector2 resultSize = base.PreferredSize;
                string textToMeasure = Text;
                if (string.IsNullOrEmpty(textToMeasure) == true)
                {
                    textToMeasure = "0";
                }
                Vector2 textSize = (Font != null ? (Vector2)Font.MeasureString(textToMeasure) : Vector2.Zero);
                if (textSize.X > resultSize.X)
                {
                    resultSize.X = textSize.X;
                }
                if (textSize.Y > resultSize.Y)
                {
                    resultSize.Y = textSize.Y;
                }
                return resultSize;
            }
        }

        public UITextField(string setText = "", TextureFont? setFont = null)
        {
            firstKeyRepeat = true;
            lastPressedKey = Key.Unknown;
            blinkTimeStartTotalSeconds = -1;
            lastPressedKeyTimestamp = -1;
            cursorPos = 0;
            Text = setText;
            Font = setFont ?? AlkaronCoreGame.Core.DefaultFont;
            Padding = 10;
            renderCursor = false;

            Width = 100;
            Height = PreferredSize.Y;

            Focusable = true;

            if (string.IsNullOrEmpty(Text) == false)
            {
                cursorPos = Text.Length;
            }
        }

        protected override void Draw(RenderContext renderContext)
        {
            base.Draw(renderContext);

            string textToDisplay = Text;
            RgbaFloat textColor = new RgbaFloat(new Vector4(Vector3.One, CompositeAlpha));

            if (string.IsNullOrWhiteSpace(textToDisplay))
            {
                textColor = new RgbaFloat(new Vector4(new Vector3(0.5f), CompositeAlpha));
                textToDisplay = ShadowText;
            }

            Vector2 screenPos = ScreenPosition;
            RectangleF rect = new RectangleF(screenPos.X, screenPos.Y, Width, Height);

            RgbaFloat borderColor = new RgbaFloat(new Vector4(BorderColor.R, BorderColor.G, BorderColor.B, CompositeAlpha));
            ScreenQuad.RenderRectangle(renderContext, rect, borderColor, BorderWidth);

            Vector2 fullSize = (Font != null ? (Vector2)Font.MeasureString(textToDisplay) : Vector2.Zero);

            // Draw Text (either normal or shadow text)
            Vector2 position = new Vector2(0, screenPos.Y - 3);
            position.X = screenPos.X + Padding;
            position.Y = screenPos.Y + (Height / 2.0f - fullSize.Y / 2.0f);
            TextRenderer.RenderText(renderContext, textToDisplay, position.X, position.Y, textColor, Font);

            // Draw cursor at location of Text
            if (HasCapturedKeyboardFocus() &&
                renderCursor)
            {
                float cursorLocation = 0.0f;
                if (Text != null)
                {
                    string textSubstring = Text.Substring(0, cursorPos);
                    Vector2 textSubstringLength = (Font != null ? (Vector2)Font.MeasureString(textSubstring) : Vector2.Zero);
                    cursorLocation = textSubstringLength.X;
                }
                RectangleF cursorRect = new RectangleF(position.X + cursorLocation, position.Y, 2, fullSize.Y);
                ScreenQuad.FillRectangle(renderContext, cursorRect, RgbaFloat.White);
            }
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            bool hasKeyboardFocus = HasCapturedKeyboardFocus();

            if (hasKeyboardFocus)
            {
                if (blinkTimeStartTotalSeconds < 0)
                {
                    blinkTimeStartTotalSeconds = totalTimeGameTime;
                }

                double globalDeltaTime = totalTimeGameTime - blinkTimeStartTotalSeconds;
                double timeScale = 1.4;
                globalDeltaTime *= timeScale;
                renderCursor = (int)(globalDeltaTime) % 2 == 0;
            }
            else
            {
                renderCursor = false;
            }

            if (lastPressedKey != Key.Unknown)
            {
                double globalDeltaTime = totalTimeGameTime - lastPressedKeyTimestamp;
                double limit = 0.1;
                if (firstKeyRepeat == true)
                {
                    limit = 0.5;
                }
                if (globalDeltaTime > limit)
                {
                    if (firstKeyRepeat == true)
                    {
                        firstKeyRepeat = false;
                    }

                    KeyPressed(lastPressedKey, deltaTime);
                }
            }
        }

        public override void ReceiveFocus(double deltaTime)
        {
            base.ReceiveFocus(deltaTime);

            blinkTimeStartTotalSeconds = totalTimeGameTime;
        }

        public override void RelinquishFocus()
        {
            base.RelinquishFocus();

            lastPressedKey = Key.Unknown;
            blinkTimeStartTotalSeconds = -1;
        }

        protected internal override bool PointerUp(Vector2 position, PointerType pointerType, double gameTime)
        {
            ReceiveFocus(gameTime);
            return base.PointerUp(position, pointerType, gameTime);
        }

        private char GetSpecialChar(Key key)
        {
            switch (key)
            {
                case Key.Plus:
                case Key.KeypadPlus:
                    return '+';
                case Key.Minus:
                case Key.KeypadMinus:
                    return '-';

                case Key.KeypadPeriod:
                    return '.';
                case Key.Comma:
                    return ',';
                case Key.Semicolon:
                    return ';';

                default:
                    return (char)0;
            }
        }

        protected internal override bool KeyReleased(Key key, double gameTime)
        {
            if (base.KeyReleased(key, gameTime) == true)
            {
                return true;
            }

            if (key == lastPressedKey)
            {
                lastPressedKey = Key.Unknown;
            }

            return true;
        }

        protected internal override bool KeyPressed(Key key, double gameTime)
        {
            if (base.KeyPressed(key, gameTime) == true ||
                Text == null)
            {
                return true;
            }

            // Reset blinking cursor
            blinkTimeStartTotalSeconds = totalTimeGameTime;

            if (lastPressedKey != key)
            {
                firstKeyRepeat = true;
            }

            lastPressedKey = key;
            lastPressedKeyTimestamp = totalTimeGameTime;

            if (key == Key.Left)
            {
                cursorPos--;
                if (cursorPos < 0)
                {
                    cursorPos = 0;
                }
            }
            if (key == Key.Right)
            {
                if (string.IsNullOrEmpty(Text) == false)
                {
                    cursorPos++;
                    if (cursorPos > Text.Length)
                    {
                        cursorPos = Text.Length;
                    }
                }
            }

            char keyCode = InputManager.GetCharForKey(key);
            char specialChar = GetSpecialChar(key);
            if (char.IsLetterOrDigit(keyCode))
            {
                bool isShiftPressed = AlkaronCoreGame.Core.SceneManager.InputManager.IsKeyPressed(Key.LShift) ||
                    AlkaronCoreGame.Core.SceneManager.InputManager.IsKeyPressed(Key.RShift);

                if (isShiftPressed)
                {
                    keyCode = char.ToUpperInvariant(keyCode);
                }
                else
                {
                    keyCode = char.ToLowerInvariant(keyCode);
                }

                Text = Text.Insert(cursorPos, keyCode.ToString());
                cursorPos++;
            }
            if (key == Key.Space)
            {
                Text = Text.Insert(cursorPos, keyCode.ToString());
                cursorPos++;
            }
            if (key == Key.Home)
            {
                cursorPos = 0;
            }
            if (key == Key.End)
            {
                cursorPos = Text.Length;
            }
            if (char.IsPunctuation(specialChar))
            {
                Text = Text.Insert(cursorPos, specialChar.ToString());
                cursorPos++;
            }
            if (key == Key.Back &&
               cursorPos > 0)
            {
                Text = Text.Remove(cursorPos - 1, 1);
                cursorPos--;
            }
            if (key == Key.Delete &&
                cursorPos < Text.Length)
            {
                Text = Text.Remove(cursorPos, 1);
            }

            return true;
        }
    }
}
