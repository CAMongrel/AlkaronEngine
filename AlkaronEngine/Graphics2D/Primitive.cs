using System;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Graphics2D
{
   public class Primitive
   {
      public Primitive()
      {
      }

      public static void DrawRectangle(IRenderConfiguration renderConfig,
                                      float x, float y, float width, float height,
                                      Color FillColor)
      {
         DrawRectangle(renderConfig, new Vector2(x, y), new Vector2(width, height), FillColor);
      }

		public static void DrawRectangle(IRenderConfiguration renderConfig,
                                       Vector2 position, Vector2 size,
										         Color FillColor)
		{
         ScreenQuad.RenderQuad(position, size, FillColor, 0, 
                               Texture.SingleWhite.NativeTexture, renderConfig);
      }
   }
}
