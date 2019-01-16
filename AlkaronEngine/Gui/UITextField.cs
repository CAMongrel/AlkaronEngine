using AlkaronEngine.Graphics2D;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlkaronEngine.Gui
{
    public class UITextField : UIBaseComponent
    {
        private double blinkTimeStartTotalSeconds;

        public SpriteFont Font { get; set; }

        public Color BorderColor { get; set; }
        public int BorderWidth { get; set; }

        public int Padding { get; set; }

        public string ShadowText { get; set; }
        public string Text { get; set; }

        private int cursorPos;
        private bool renderCursor;
        private bool firstKeyRepeat;
        private Keys lastPressedKey;
        private double lastPressedKeyTimestamp;

        public UITextField(IRenderConfiguration renderConfig, SpriteFont setFont = null, string setText = "")
            : base(renderConfig)
        {
            firstKeyRepeat = true;
            lastPressedKey = Keys.None;
            blinkTimeStartTotalSeconds = -1;
            lastPressedKeyTimestamp = -1;
            cursorPos = 0;
            Text = setText;
            Font = setFont ?? renderConfig.RenderManager.EngineFont;
            Padding = 10;
            renderCursor = false;

            Focusable = true;

            if (string.IsNullOrEmpty(Text) == false)
            {
                cursorPos = Text.Length;
            }
        }

        protected override void Draw()
        {
            base.Draw();

            string textToDisplay = Text;
            Color textColor = Color.FromNonPremultiplied(new Vector4(Vector3.One, CompositeAlpha));

            if (string.IsNullOrWhiteSpace(textToDisplay))
            {
                textColor = Color.FromNonPremultiplied(new Vector4(new Vector3(0.5f), CompositeAlpha));
                textToDisplay = ShadowText;
            }

            Vector2 screenPos = ScreenPosition;
            RectangleF rect = new RectangleF(screenPos.X, screenPos.Y, Width, Height);

            Color borderColor = Color.FromNonPremultiplied(new Vector4(BorderColor.ToVector3(), CompositeAlpha));
            Graphics2D.Texture.RenderRectangle(renderConfig, rect, borderColor, BorderWidth);

            Vector2 fullSize = (Font != null ? (Vector2)Font.MeasureString(textToDisplay) : Vector2.Zero);

            // Draw Text (either normal or shadow text)
            Vector2 position = new Vector2(0, screenPos.Y - 3);
            position.X = screenPos.X + Padding;
            position.Y = screenPos.Y + (Height / 2.0f - fullSize.Y / 2.0f);
            FontRenderer.DrawStringDirect(renderConfig, Font, textToDisplay, position.X, position.Y, textColor, 1.0f, CompositeRotation);

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
                Graphics2D.Texture.FillRectangle(renderConfig, cursorRect, Color.White);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool hasKeyboardFocus = HasCapturedKeyboardFocus();

            if (hasKeyboardFocus)
            {
                if (blinkTimeStartTotalSeconds < 0)
                {
                    blinkTimeStartTotalSeconds = gameTime.TotalGameTime.TotalSeconds;
                }

                double deltaTime = gameTime.TotalGameTime.TotalSeconds - blinkTimeStartTotalSeconds;
                double timeScale = 1.4;
                deltaTime *= timeScale;
                renderCursor = (int)(deltaTime) % 2 == 0;
            }
            else
            {
                renderCursor = false;
            }

            if (lastPressedKey != Keys.None)
            {
                double deltaTime = gameTime.TotalGameTime.TotalSeconds - lastPressedKeyTimestamp;
                double limit = 0.1;
                if (firstKeyRepeat == true)
                {
                    limit = 0.5;
                }
                if (deltaTime > limit)
                {
                    if (firstKeyRepeat == true)
                    {
                        firstKeyRepeat = false;
                    }

                    KeyPressed(lastPressedKey, gameTime);
                }
            }
        }

        public override void ReceiveFocus(GameTime gameTime)
        {
            base.ReceiveFocus(gameTime);

            blinkTimeStartTotalSeconds = gameTime?.TotalGameTime.TotalSeconds ?? -1;
        }

        public override void RelinquishFocus()
        {
            base.RelinquishFocus();

            lastPressedKey = Keys.None;
            blinkTimeStartTotalSeconds = -1;
        }

        protected internal override bool PointerUp(Vector2 position, PointerType pointerType, GameTime gameTime)
        {
            ReceiveFocus(gameTime);
            return base.PointerUp(position, pointerType, gameTime);
        }

        private char GetSpecialChar(Keys key)
        {
            switch (key)
            {
                case Keys.OemPlus:
                    return '+';
                case Keys.OemMinus:
                    return '-';

                case Keys.OemPeriod:
                    return '.';
                case Keys.OemComma:
                    return ',';
                case Keys.OemSemicolon:
                    return ';';

                default:
                    return (char)0;
            }
        }

        protected internal override bool KeyReleased(Keys key, GameTime gameTime)
        {
            if (base.KeyReleased(key, gameTime) == true)
            {
                return true;
            }

            if (key == lastPressedKey)
            {
                lastPressedKey = Keys.None;
            }

            return true;
        }

        protected internal override bool KeyPressed(Keys key, GameTime gameTime)
        {
            if (base.KeyPressed(key, gameTime) == true ||
                Text == null)
            {
                return true;
            }

            // Reset blinking cursor
            blinkTimeStartTotalSeconds = gameTime.TotalGameTime.TotalSeconds;

            if (lastPressedKey != key)
            {
                firstKeyRepeat = true;
            }

            lastPressedKey = key;
            lastPressedKeyTimestamp = gameTime.TotalGameTime.TotalSeconds;

            if (key == Keys.Left)
            {
                cursorPos--;
                if (cursorPos < 0)
                {
                    cursorPos = 0;
                }
            }
            if (key == Keys.Right)
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

            char keyCode = (char)key;
            char specialChar = GetSpecialChar(key);
            if (char.IsLetterOrDigit(keyCode))
            {
                bool isShiftPressed = AlkaronCoreGame.Core.SceneManager.InputManager.IsKeyPressed(Keys.LeftShift) ||
                                                     AlkaronCoreGame.Core.SceneManager.InputManager.IsKeyPressed(Keys.RightShift);

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
            if (key == Keys.Space)
            {
                Text = Text.Insert(cursorPos, keyCode.ToString());
                cursorPos++;
            }
            if (key == Keys.Home)
            {
                cursorPos = 0;
            }
            if (key == Keys.End)
            {
                cursorPos = Text.Length;
            }
            if (char.IsPunctuation(specialChar))
            {
                Text = Text.Insert(cursorPos, specialChar.ToString());
                cursorPos++;
            }
            if (key == Keys.Back &&
               cursorPos > 0)
            {
                Text = Text.Remove(cursorPos - 1, 1);
                cursorPos--;
            }
            if (key == Keys.Delete &&
                cursorPos < Text.Length)
            {
                Text = Text.Remove(cursorPos, 1);
            }

            return true;
        }
    }
}
