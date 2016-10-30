// Author: Henning
// Project: WinWarEngine
// Path: D:\Projekte\Henning\C#\WinWarCS\WinWarEngine\Graphics
// Creation date: 27.11.2009 20:22
// Last modified: 27.11.2009 22:25

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using RectangleF = AlkaronEngine.Util.RectangleF;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace AlkaronEngine.Graphics
{
   public class Texture : IDisposable
   {
      #region Constants

      /// <summary>
      /// Create a matrix that transforms the screen space position (0..1)
      /// into device space (-1..+1)
      /// </summary>
      static readonly Matrix DeviceTransformMatrix =
         Matrix.CreateScale(2, 2, 0) *
         Matrix.CreateTranslation(-1, -1, 0) *
         Matrix.CreateScale(1, -1, 1);

      #endregion

      #region Variables
      private Texture2D nativeTexture = null;
      public int Width { get; private set; }
      public int Height { get; private set; }

      private IRenderConfiguration renderConfig;

      public static Texture SingleWhite { get; internal set; }
      #endregion

      #region Constructor

      public Texture(IRenderConfiguration setRenderConfig, int width, int height, byte[] data = null)
      {
         if (setRenderConfig == null)
         {
            throw new ArgumentNullException(nameof(setRenderConfig));
         }
         renderConfig = setRenderConfig;

         Width = width;
         Height = height;

         if (renderConfig.RequiresPowerOfTwoTextures)
         {
            width = (int)NextPowerOfTwo((uint)width);
            height = (int)NextPowerOfTwo((uint)height);
         }

         nativeTexture = new Texture2D(renderConfig.GraphicsDevice, width, height, false, SurfaceFormat.Color);
         SetData(data);
      }

      #endregion

      #region IDisposable implementation

      public void Dispose()
      {
         if (nativeTexture != null)
         {
            nativeTexture.Dispose();
            nativeTexture = null;
         }
      }

      #endregion

      #region Utility
      private uint NextPowerOfTwo(uint val)
      {
         // Taken from http://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
         val--;
         val |= val >> 1;
         val |= val >> 2;
         val |= val >> 4;
         val |= val >> 8;
         val |= val >> 16;
         val++;
         return val;
      }
      #endregion

      #region SetData
      public void SetData(byte[] data)
      {
         if (data == null)
         {
            return;
         }

         if (renderConfig.RequiresPowerOfTwoTextures == false)
         {
            nativeTexture.SetData<byte>(data);
         }
         else
         {
            nativeTexture.SetData<byte>(0, new Rectangle(0, 0, this.Width, this.Height), data, 0, data.Length);
         }
      }

      public void SetData(Color[] data)
      {
         if (data == null)
         {
            return;
         }

         if (renderConfig.RequiresPowerOfTwoTextures == false)
         {
            nativeTexture.SetData<Color>(data);
         }
         else
         {
            nativeTexture.SetData<Color>(0, new Rectangle(0, 0, this.Width, this.Height), data, 0, data.Length);
         }
      }
      #endregion

      #region RenderOnScreen

      internal void RenderOnScreen(float x, float y)
      {
         RenderOnScreen(x, y, Color.White);
      }

      internal void RenderOnScreen(float x, float y, Color color)
      {
         RenderOnScreen(new RectangleF(0, 0, (float)this.Width, (float)this.Height),
            new RectangleF(x, y, (float)this.Width, (float)this.Height), color);
      }

      internal void RenderOnScreen(RectangleF display_rect)
      {
         RenderOnScreen(display_rect, false, false);
      }

      internal void RenderOnScreen(RectangleF display_rect, bool flipX, bool flipY)
      {
         RectangleF sourceRect = new RectangleF(
                                    flipX ? (float)this.Width : 0,
                                    flipY ? (float)this.Height : 0,
                                    flipX ? -(float)this.Width : (float)this.Width,
                                    flipY ? -(float)this.Height : (float)this.Height);

         RenderOnScreen(sourceRect,
            new RectangleF(display_rect.X, display_rect.Y, display_rect.Width, display_rect.Height),
            Color.White, flipX, flipY);
      }

      internal void RenderOnScreen(float x, float y, float width, float height)
      {
         RenderOnScreen(x, y, width, height, Color.White);
      }

      internal void RenderOnScreen(float x, float y, float width, float height, Color color)
      {
         RenderOnScreen(new RectangleF(0, 0, (float)this.Width, (float)this.Height),
            new RectangleF(x, y, width, height), color);
      }

      internal void RenderOnScreen(RectangleF sourceRect, RectangleF destRect)
      {
         RenderOnScreen(sourceRect, destRect, Color.White);
      }

      internal void RenderOnScreen(RectangleF sourceRect, RectangleF destRect, Color col, bool flipX = false, bool flipY = false)
      {
         Vector2 Scale = renderConfig.Scale;
         Vector2 ScaledOffset = renderConfig.ScaledOffset;

         Rectangle srcRect = new Rectangle((int)sourceRect.X, (int)sourceRect.Y, (int)sourceRect.Width, (int)sourceRect.Height);
         Vector2 position = new Vector2(ScaledOffset.X + destRect.X * Scale.X,
                               ScaledOffset.Y + destRect.Y * Scale.Y);
         Vector2 scale = new Vector2(flipX ? -Scale.X : Scale.X, flipY ? -Scale.Y : Scale.Y);
         scale = new Vector2(Scale.X, Scale.Y);
         scale.X *= destRect.Width / sourceRect.Width;
         scale.Y *= destRect.Height / sourceRect.Height;

         renderConfig.RenderManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
         renderConfig.RenderManager.SpriteBatch.Draw(nativeTexture, position, srcRect, col, 0, Vector2.Zero, scale, SpriteEffects.None, 1.0f);
         renderConfig.RenderManager.SpriteBatch.End();
      }

      internal static void RenderRectangle(IRenderConfiguration renderConfig, RectangleF destRect, Color col, int BorderWidth = 1)
      {
         if (renderConfig == null)
         {
            throw new ArgumentNullException(nameof(renderConfig));
         }

         Vector2 Scale = renderConfig.Scale;
         Vector2 ScaledOffset = renderConfig.ScaledOffset;

         destRect = new RectangleF(ScaledOffset.X + (int)(destRect.X * Scale.X), ScaledOffset.Y + (int)(destRect.Y * Scale.Y),
            (int)(destRect.Width * Scale.X), (int)(destRect.Height * Scale.Y));

         Rectangle singleRect = new Rectangle(0, 0, 1, 1);
         Rectangle leftRect = new Rectangle((int)destRect.X, (int)destRect.Y, BorderWidth, (int)destRect.Height);
         Rectangle topRect = new Rectangle((int)destRect.X, (int)destRect.Y, (int)destRect.Width, BorderWidth);
         Rectangle rightRect = new Rectangle((int)destRect.X + (int)destRect.Width - BorderWidth, (int)destRect.Y, BorderWidth, (int)destRect.Height);
         Rectangle bottomRect = new Rectangle((int)destRect.X, (int)destRect.Y + (int)destRect.Height - BorderWidth, (int)destRect.Width, BorderWidth);

         renderConfig.RenderManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
         renderConfig.RenderManager.SpriteBatch.Draw(Texture.SingleWhite.nativeTexture, leftRect, singleRect, col);
         renderConfig.RenderManager.SpriteBatch.Draw(Texture.SingleWhite.nativeTexture, topRect, singleRect, col);
         renderConfig.RenderManager.SpriteBatch.Draw(Texture.SingleWhite.nativeTexture, rightRect, singleRect, col);
         renderConfig.RenderManager.SpriteBatch.Draw(Texture.SingleWhite.nativeTexture, bottomRect, singleRect, col);
         renderConfig.RenderManager.SpriteBatch.End();
      }

      #endregion      

      #region Unit testing

      internal static void TestLoadAndRender()
      {
         throw new NotImplementedException();
         /*WWTexture tex = null;
			
         TestGame.Start("TestLoadAndRender",
            delegate
            {
               WarResource res = WarFile.GetResource(243);
               WarResource pal = WarFile.GetResource(260);
               ImageResource img = new ImageResource(res, pal);
               tex = Texture.FromImageResource(img);					
            },
            delegate
            {
               tex.RenderOnScreen(0, 0, tex.Width, tex.Height);
            });
            */
      }

      #endregion

   }
}
