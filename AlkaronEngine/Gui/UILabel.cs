using AlkaronEngine.Graphics;
using AlkaronEngine.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Gui
{
   public class UILabel : UIBaseComponent
   {
      #region Properties
      public string Text { get; set; }
      public SpriteFont Font { get; set; }
      public TextAlignHorizontal TextAlign { get; set; }
      #endregion

      #region Constructor
      public UILabel(IRenderConfiguration renderConfig, string setText, SpriteFont setFont)
         : base(renderConfig)
      {
         Text = setText;
         Font = setFont;
         TextAlign = TextAlignHorizontal.Center;
      }
      #endregion

      #region Render
      public override void Draw()
      {
         base.Draw();

         Vector2 screenPos = ScreenPosition;

         Color col = Color.FromNonPremultiplied(new Vector4(Vector3.One, CompositeAlpha));

         Vector2 fullSize = Font.MeasureString(Text);

         Vector2 position = new Vector2(0, screenPos.Y - 3);
         if (Height > 0)
         {
            position.Y = screenPos.Y + (Height / 2.0f - fullSize.Y / 2.0f);
         }

         switch (TextAlign)
         {
            case TextAlignHorizontal.Center:
               position.X = screenPos.X + (Width / 2.0f - fullSize.X / 2.0f);
               break;

            case TextAlignHorizontal.Left:
               position.X = screenPos.X;
               break;

            case TextAlignHorizontal.Right:
               position.X = screenPos.X + (Width - fullSize.X);
               break;
         }

         renderConfig.RenderManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);

         FontRenderer.DrawString(renderConfig, Font, Text, position.X, position.Y, col);

         renderConfig.RenderManager.SpriteBatch.End();
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
