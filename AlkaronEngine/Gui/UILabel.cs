using System.Diagnostics.Contracts;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Gui
{
    public class UILabel : UIBaseComponent
    {
        #region Properties
        public string Text { get; set; }
        public SpriteFont Font { get; set; }

        public bool AutoScaleFont { get; set; }

        /// <summary>
        /// Gets or sets the horizontal text alignment.
        /// </summary>
        public UITextAlignHorizontal TextAlignHorizontal { get; set; }

        /// <summary>
        /// Gets or sets the vertical text alignment.
        /// </summary>
        public UITextAlignVertical TextAlignVertical { get; set; }

        public override Vector2 PreferredSize
        {
            get
            {
                Vector2 resultSize = base.PreferredSize;
                Vector2 textSize = (Font != null ? (Vector2)Font.MeasureString(Text) : Vector2.Zero);
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
        public UILabel(IRenderConfiguration renderConfig, string setText, SpriteFont setFont)
           : base(renderConfig)
        {
            Text = setText;
            Font = setFont;
            TextAlignHorizontal = UITextAlignHorizontal.Center;
            TextAlignVertical = UITextAlignVertical.Center;
            AutoScaleFont = false;
        }
        #endregion

        #region Render
        protected override void Draw()
        {
            base.Draw();

            Vector2 screenPos = ScreenPosition;

            Color col = Color.FromNonPremultiplied(new Vector4(Vector3.One, CompositeAlpha));

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

            FontRenderer.DrawStringDirect(renderConfig, Font, Text, position.X, position.Y, col, textScale, CompositeRotation);

            //
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
