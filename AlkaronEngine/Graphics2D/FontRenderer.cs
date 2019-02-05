using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using AlkaronEngine.Gui;

namespace AlkaronEngine.Graphics2D
{
    public static class FontRenderer
    {
        /// <summary>
        /// Draws a text at the specified position without a begin/end. Suitable for multiple draw events.
        /// </summary>
        public static void DrawString(IRenderConfiguration renderConfig, SpriteFont font, string text, float x, float y, Color color)
        {
            if (renderConfig == null)
            {
                throw new ArgumentNullException(nameof(renderConfig));
            }

            Vector2 Scale = renderConfig.Scale;
            Vector2 ScaledOffset = renderConfig.ScaledOffset;

            renderConfig.PrimitiveRenderManager.SpriteBatch.DrawString(font, text, new Vector2(ScaledOffset.X + (x * Scale.X), ScaledOffset.Y + (y * Scale.Y)), color, 0,
                Vector2.Zero, new Vector2(Scale.X, Scale.Y), SpriteEffects.None, 1.0f);
        }

        /// <summary>
        /// Draws a text at the specified position nested in a begin/end. Suitable for quick single line drawing.
        /// Uses default scaling
        /// </summary>
        public static void DrawStringDirect(IRenderConfiguration renderConfig, SpriteFont font, string text, float x, float y, Color color)
        {
            DrawStringDirect(renderConfig, font, text, x, y, color, 1.0f);
        }

        /// <summary>
        /// Draws a text at the specified position nested in a begin/end. Suitable for quick single line drawing.
        /// </summary>
        public static void DrawStringDirect(IRenderConfiguration renderConfig, SpriteFont font, string text, float x, float y, Color color, float scale, float deg_rotation = 0)
        {
            if (renderConfig == null)
            {
                throw new ArgumentNullException(nameof(renderConfig));
            }

            Vector2 Scale = renderConfig.Scale;
            Vector2 ScaledOffset = renderConfig.ScaledOffset;
            float radRotation = deg_rotation * ((float)Math.PI / 180.0f);

            Vector2 pos = new Vector2(ScaledOffset.X + (x * Scale.X), ScaledOffset.Y + (y * Scale.Y));

            Vector2 scaleVec = new Vector2(Scale.X * scale, Scale.Y * scale);

            renderConfig.PrimitiveRenderManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            renderConfig.PrimitiveRenderManager.SpriteBatch.DrawString(font, text, pos,
                                                              color, radRotation, Vector2.Zero, scaleVec,
                                                              SpriteEffects.None, 1.0f);

            renderConfig.PrimitiveRenderManager.SpriteBatch.End();
        }

        private static Vector2 MeasureString(string text, SpriteFont font1)
        {
            if (font1 != null)
            {
                return font1.MeasureString(text);
            }
            else
            {
                return Vector2.Zero;
            }
        }

        private static string[] SplitText(string text, int maxWidth, SpriteFont font1)
        {
            List<string> result = new List<string>();

            int spaceSize = (int)MeasureString(" ", font1).X;

            string[] paragraphs = text.Split('\n');
            for (int i = 0; i < paragraphs.Length; i++)
            {
                string[] words = paragraphs[i].Split(' ');

                string line = string.Empty;
                int lineLength = 0;

                for (int j = 0; j < words.Length; j++)
                {
                    Vector2 wordSize = MeasureString(words[j], font1);

                    if (lineLength + spaceSize + wordSize.X > maxWidth)
                    {
                        if (lineLength == 0)
                        {
                            // Special case: Single word exceeds line length
                            result.Add(words[j]);
                        }
                        else
                        {
                            // Normal case, add word to next line
                            result.Add(line);

                            line = words[j];
                            lineLength = (int)wordSize.X;
                        }
                    }
                    else
                    {
                        line += " " + words[j];
                        lineLength += spaceSize + (int)wordSize.X;
                    }
                }

                result.Add(line);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Draws a text at the specified position nested in a begin/end. Suitable for quick text drawing.
        /// Automatically fits the text into the specifiec space. 
        /// </summary>
        public static void DrawStringDirect(IRenderConfiguration renderConfig, SpriteFont font, string text,
                                            float x, float y, float width, float height, Color color,
                                                        UITextAlignHorizontal horizontalAlignment = UITextAlignHorizontal.Center,
                                                        UITextAlignVertical verticalAlignment = UITextAlignVertical.Center)
        {
            DrawStringDirectInternal(renderConfig, font, text, x, y, width, height, color,
                                    horizontalAlignment, verticalAlignment);
        }

        private static void DrawStringDirectInternal(IRenderConfiguration renderConfig, SpriteFont font1, 
                                                     string text, float x, float y, float width, float height, Color color,
                                                     UITextAlignHorizontal horizontalAlignment,
                                                     UITextAlignVertical verticalAlignment)
        {
            if (renderConfig == null)
            {
                throw new ArgumentNullException(nameof(renderConfig));
            }

            Vector2 Scale = renderConfig.Scale;
            Vector2 ScaledOffset = renderConfig.ScaledOffset;

            string[] lines = SplitText(text, (int)width, font1);
            int lineHeight = font1 != null ? font1.LineSpacing : 0;
            int totalHeight = lineHeight * lines.Length;

            renderConfig.PrimitiveRenderManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            float yGap = -2;
            float yPos = 0;

            switch (verticalAlignment)
            {
                case UITextAlignVertical.Top:
                    yPos = y;
                    break;

                case UITextAlignVertical.Center:
                    yPos = y + (height / 2 - totalHeight / 2);
                    break;

                case UITextAlignVertical.Bottom:
                    yPos = y + (height - totalHeight);
                    break;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 lineSize = MeasureString(lines[i], font1);
                float xPos = 0;

                switch (horizontalAlignment)
                {
                    case UITextAlignHorizontal.Left:
                        xPos = x;
                        break;

                    case UITextAlignHorizontal.Center:
                        xPos = x + (width / 2 - lineSize.X / 2);
                        break;

                    case UITextAlignHorizontal.Right:
                        xPos = x + (width - lineSize.X);
                        break;
                }

                if (font1 != null)
                {
                    renderConfig.PrimitiveRenderManager.SpriteBatch.DrawString(font1,
                       lines[i],
                       new Vector2(ScaledOffset.X + (xPos * Scale.X), ScaledOffset.Y + (yPos * Scale.Y)),
                       color,
                       0,
                       Vector2.Zero,
                       new Vector2(Scale.X, Scale.Y),
                       SpriteEffects.None,
                       1.0f);
                }

                yPos += lineHeight + yGap;
            }

            renderConfig.PrimitiveRenderManager.SpriteBatch.End();
        }
    }
}
