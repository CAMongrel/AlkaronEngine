using System;

namespace AlkaronEngine.Graphics2D
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

        #region Properties
        public Texture2D NativeTexture
        {
            get { return nativeTexture; }
            set
            {
                nativeTexture = value;
                Width = nativeTexture.Width;
                Height = nativeTexture.Height;
            }
        }
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

        public Texture(IRenderConfiguration setRenderConfig, Texture2D setNativeTexture)
        {
            if (setRenderConfig == null)
            {
                throw new ArgumentNullException(nameof(setRenderConfig));
            }
            if (setNativeTexture == null)
            {
                throw new ArgumentNullException(nameof(setNativeTexture));
            }
            renderConfig = setRenderConfig;

            nativeTexture = setNativeTexture;
            Width = nativeTexture.Width;
            Height = nativeTexture.Height;
        }

        public Texture(IRenderConfiguration setRenderConfig, string contentFilename)
        {
            if (setRenderConfig == null)
            {
                throw new ArgumentNullException(nameof(setRenderConfig));
            }
            if (contentFilename == null)
            {
                throw new ArgumentNullException(nameof(contentFilename));
            }
            renderConfig = setRenderConfig;

            using (var stream = AlkaronCoreGame.Core.AlkaronContent.OpenContentStream(contentFilename))
            {
                nativeTexture = Texture2D.FromStream(renderConfig.GraphicsDevice, stream);
            }

            Width = nativeTexture.Width;
            Height = nativeTexture.Height;
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

        public void SetData(byte[] data, Rectangle rectToWrite)
        {
            if (data == null)
            {
                return;
            }

            nativeTexture.SetData<byte>(0, rectToWrite, data, 0, data.Length);
        }

        #endregion

        #region RenderOnScreen

        public void RenderOnScreen(float x, float y)
        {
            RenderOnScreen(x, y, Color.White);
        }

        public void RenderOnScreen(float x, float y, Color color, float rotation = 0.0f)
        {
            RenderOnScreen(new RectangleF(0, 0, (float)this.Width, (float)this.Height),
                           new RectangleF(x, y, (float)this.Width, (float)this.Height), color, rotation);
        }

        public void RenderOnScreen(RectangleF display_rect)
        {
            RenderOnScreen(display_rect, false, false);
        }

        public void RenderOnScreen(RectangleF display_rect, bool flipX, bool flipY, float rotation = 0.0f)
        {
            RectangleF sourceRect = new RectangleF(
                                       flipX ? (float)this.Width : 0,
                                       flipY ? (float)this.Height : 0,
                                       flipX ? -(float)this.Width : (float)this.Width,
                                       flipY ? -(float)this.Height : (float)this.Height);

            RenderOnScreen(sourceRect,
               new RectangleF(display_rect.X, display_rect.Y, display_rect.Width, display_rect.Height),
               Color.White, rotation, flipX, flipY);
        }

        public void RenderOnScreen(float x, float y, float width, float height, float rotation = 0.0f)
        {
            RenderOnScreen(x, y, width, height, Color.White, rotation);
        }

        public void RenderOnScreen(float x, float y, float width, float height, Color color, float rotation = 0.0f)
        {
            RenderOnScreen(new RectangleF(0, 0, (float)this.Width, (float)this.Height),
                           new RectangleF(x, y, width, height), color, rotation);
        }

        public void RenderOnScreen(RectangleF sourceRect, RectangleF destRect, float rotation = 0.0f)
        {
            RenderOnScreen(sourceRect, destRect, Color.White, rotation);
        }

        public void RenderOnScreen(RectangleF sourceRect, RectangleF destRect, Color col, float rotation, bool flipX = false, bool flipY = false)
        {
            Vector2 Scale = renderConfig.Scale;
            Vector2 ScaledOffset = renderConfig.ScaledOffset;

            Vector2 scale = new Vector2(flipX ? -Scale.X : Scale.X, flipY ? -Scale.Y : Scale.Y);
            scale = new Vector2(Scale.X, Scale.Y);
            scale.X *= destRect.Width / sourceRect.Width;
            scale.Y *= destRect.Height / sourceRect.Height;

            float radRotation = rotation * ((float)Math.PI / 180.0f);

            ScreenQuad.RenderQuad(new Vector2(destRect.X, destRect.Y),
                                  new Vector2(destRect.Width, destRect.Height),
                                  col, radRotation, this.nativeTexture,
                                  renderConfig);
        }

        public static void RenderRectangle(IRenderConfiguration renderConfig, RectangleF destRect, Color col, int BorderWidth = 1)
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

            renderConfig.PrimitiveRenderManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
            renderConfig.PrimitiveRenderManager.SpriteBatch.Draw(Texture.SingleWhite.nativeTexture, leftRect, singleRect, col);
            renderConfig.PrimitiveRenderManager.SpriteBatch.Draw(Texture.SingleWhite.nativeTexture, topRect, singleRect, col);
            renderConfig.PrimitiveRenderManager.SpriteBatch.Draw(Texture.SingleWhite.nativeTexture, rightRect, singleRect, col);
            renderConfig.PrimitiveRenderManager.SpriteBatch.Draw(Texture.SingleWhite.nativeTexture, bottomRect, singleRect, col);
            renderConfig.PrimitiveRenderManager.SpriteBatch.End();
        }

        public static void FillRectangle(IRenderConfiguration renderConfig, RectangleF destRect, Color col)
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
            Rectangle destinationRect = new Rectangle((int)destRect.X, (int)destRect.Y, (int)destRect.Width, (int)destRect.Height);

            renderConfig.PrimitiveRenderManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
            renderConfig.PrimitiveRenderManager.SpriteBatch.Draw(Texture.SingleWhite.nativeTexture, destinationRect, singleRect, col);
            renderConfig.PrimitiveRenderManager.SpriteBatch.End();
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
