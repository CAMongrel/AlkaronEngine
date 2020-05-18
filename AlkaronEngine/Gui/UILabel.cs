using AlkaronEngine.Assets.TextureFonts;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D;
using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Gui
{
    public class UILabel : UIBaseComponent
    {
        #region Properties
        public string Text { get; set; }
        public TextureFont Font { get; set; }

        public bool AutoScaleFont { get; set; } = false;

        public RgbaFloat ForegroundColor { get; set; } = RgbaFloat.White;

        /// <summary>
        /// Gets or sets the horizontal text alignment.
        /// </summary>
        public UITextAlignHorizontal TextAlignHorizontal { get; set; } = UITextAlignHorizontal.Center;

        /// <summary>
        /// Gets or sets the vertical text alignment.
        /// </summary>
        public UITextAlignVertical TextAlignVertical { get; set; } = UITextAlignVertical.Center;

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
        #endregion

        #region Constructor
        public UILabel(string setText, TextureFont? setFont = null)
        {
            Text = setText;
            Font = setFont ?? AlkaronCoreGame.Core.DefaultFont;
            TextAlignHorizontal = UITextAlignHorizontal.Center;
            TextAlignVertical = UITextAlignVertical.Center;

            Width = 100;
            Height = PreferredSize.Y;

            AutoScaleFont = false;
        }
        #endregion

        #region Render
        protected override void Draw(RenderContext renderContext)
        {
            base.Draw(renderContext);

            TextureFont fontToUse = this.Font;
            if (fontToUse == null)
            {
                return;
            }

            Vector2 screenPos = ScreenPosition;

            RgbaFloat col = new RgbaFloat(new Vector4(ForegroundColor.R, ForegroundColor.G, ForegroundColor.B, CompositeAlpha));

            Vector2 fullSize = (Font != null ? (Vector2)Font.MeasureString(Text) : Vector2.Zero);
            float textScale = 1.0f;
            if (AutoScaleFont)
            {
                textScale = this.Width / fullSize.X;
            }
            fullSize *= textScale;

            Vector2 position = new Vector2(0, screenPos.Y - 3);

            switch (TextAlignHorizontal)
            {
                case UITextAlignHorizontal.Center:
                    position.X = screenPos.X + (Width / 2.0f - fullSize.X / 2.0f);
                    break;

                case UITextAlignHorizontal.Left:
                    position.X = screenPos.X;
                    break;

                case UITextAlignHorizontal.Right:
                    position.X = screenPos.X + (Width - fullSize.X);
                    break;
            }

            switch (TextAlignVertical)
            {
                case UITextAlignVertical.Top:
                    position.Y = screenPos.Y;
                    break;

                case UITextAlignVertical.Center:
                    position.Y = screenPos.Y + (Height / 2.0f - fullSize.Y / 2.0f);
                    break;

                case UITextAlignVertical.Bottom:
                    position.Y = screenPos.Y + Height - fullSize.Y;
                    break;
            }

            TextRenderer.RenderText(renderContext, Text, position.X, position.Y, col, fontToUse);
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            return Text;
        }
        #endregion
    }
}