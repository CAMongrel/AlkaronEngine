using AlkaronEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AlkaronEngine.Gui.Rendering
{
   internal static class FontRenderer
   {
      /// <summary>
      /// Draws a text at the specified position without a begin/end. Suitable for multiple draw events.
      /// </summary>
      internal static void DrawString(IRenderConfiguration renderConfig, SpriteFont font, string text, float x, float y, Color color)
      {
         if (renderConfig == null)
         {
            throw new ArgumentNullException(nameof(renderConfig));
         }

         Vector2 Scale = renderConfig.Scale;
         Vector2 ScaledOffset = renderConfig.ScaledOffset;

         renderConfig.RenderManager.SpriteBatch.DrawString(font, text, new Vector2(ScaledOffset.X + (x * Scale.X), ScaledOffset.Y + (y * Scale.Y)), color, 0,
             Vector2.Zero, new Vector2(Scale.X, Scale.Y), SpriteEffects.None, 1.0f);
      }

      /// <summary>
      /// Draws a text at the specified position nested in a begin/end. Suitable for quick single line drawing.
      /// Uses default scaling
      /// </summary>
      internal static void DrawStringDirect(IRenderConfiguration renderConfig, SpriteFont font, string text, float x, float y, Color color)
      {
         DrawStringDirect (renderConfig, font, text, x, y, color, 1.0f);
      }

      /// <summary>
      /// Draws a text at the specified position nested in a begin/end. Suitable for quick single line drawing.
      /// </summary>
      internal static void DrawStringDirect(IRenderConfiguration renderConfig, SpriteFont font, string text, float x, float y, Color color, float scale)
      {
         if (renderConfig == null)
         {
            throw new ArgumentNullException(nameof(renderConfig));
         }

         Vector2 Scale = renderConfig.Scale;
         Vector2 ScaledOffset = renderConfig.ScaledOffset;

         renderConfig.RenderManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);

         renderConfig.RenderManager.SpriteBatch.DrawString(font, 
            text, 
            new Vector2(ScaledOffset.X + (x * Scale.X), ScaledOffset.Y + (y * Scale.Y)), 
            color, 
            0,
            Vector2.Zero, 
            new Vector2(Scale.X * scale, Scale.Y * scale),
            SpriteEffects.None, 
            1.0f);

         renderConfig.RenderManager.SpriteBatch.End();
      }

      /// <summary>
      /// Draws a text at the specified position nested in a begin/end. Suitable for quick text drawing.
      /// </summary>
      internal static void DrawStringDirect(IRenderConfiguration renderConfig, SpriteFont font, string text, float x, float y, float width, float height, Color color)
      {
         if (renderConfig == null)
         {
            throw new ArgumentNullException(nameof(renderConfig));
         }

         Vector2 Scale = renderConfig.Scale;
         Vector2 ScaledOffset = renderConfig.ScaledOffset;

         string[] lines = text.Split ('\n');

         renderConfig.RenderManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);

         float yGap = -2;
         float yPos = y;
         for (int i = 0; i < lines.Length; i++)
         {
            Vector2 lineSize = font.MeasureString (lines [i]);
            float xPos = x + (width / 2 - lineSize.X / 2);

            renderConfig.RenderManager.SpriteBatch.DrawString(font, 
               lines [i], 
               new Vector2(ScaledOffset.X + (xPos * Scale.X), ScaledOffset.Y + (yPos * Scale.Y)), 
               color,
               0,
               Vector2.Zero, 
               new Vector2(Scale.X, Scale.Y),
               SpriteEffects.None, 
               1.0f);

            yPos += font.LineSpacing + yGap;
         }

         renderConfig.RenderManager.SpriteBatch.End();
      }
   }
}
