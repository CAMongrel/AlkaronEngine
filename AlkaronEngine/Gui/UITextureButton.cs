#region Using directives
using AlkaronEngine.Assets.TextureFonts;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D;
using System.Numerics;
using Veldrid;
#endregion

namespace AlkaronEngine.Gui
{
    public class UITextureButton : UIButton
    {
        #region Variables
        protected Texture backgroundNotClicked;
        protected Texture backgroundClicked;
        #endregion

        #region Constructor
        public UITextureButton(string setText, TextureFont setFont,
           Texture releaseButtonTexture, Texture pressedButtonTexture)
           : base(setText, setFont)
        {
            backgroundClicked = pressedButtonTexture;
            backgroundNotClicked = releaseButtonTexture;

            Width = 0;
            Height = 0;
            if (backgroundNotClicked != null)
            {
                Width = backgroundNotClicked.Width;
                Height = backgroundNotClicked.Width;
            }
        }
        #endregion

        #region Render
        protected override void Draw(RenderContext renderContext)
        {
            Texture background;
            if (isPressed)
            {
                background = backgroundClicked;
            }
            else
            {
                background = backgroundNotClicked;
            }

            if (background == null)
            {
                return;
            }

            Vector2 screenPos = ScreenPosition;
            Vector2 screenSize = new Vector2(Width, Height);

            RgbaFloat col = new RgbaFloat(new Vector4(Vector3.One, CompositeAlpha));
            ScreenQuad.RenderQuad(renderContext, screenPos, screenSize, background, RgbaFloat.White, CompositeRotation);

            base.Draw(renderContext);
        }
        #endregion
    }
}
