using AlkaronEngine.Graphics;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Gui
{
   class UIImage : UIBaseComponent
   {
      private Texture image;

      internal UIImage(IRenderConfiguration renderConfig, Texture setImage)
         : base(renderConfig)
      {
         image = setImage;

         if (image != null)
         {
            Width = image.Width;
            Height = image.Height;
         }
      }

      public override void Draw()
      {
         base.Draw();

         if (image != null)
         {
            Vector2 screenPos = ScreenPosition;

            image.RenderOnScreen(screenPos.X, screenPos.Y, Width, Height, Color.FromNonPremultiplied(new Vector4(Vector3.One, CompositeAlpha)));
         }
      }
   }
}
