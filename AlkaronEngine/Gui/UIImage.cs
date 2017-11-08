using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Gui
{
   public enum UIImageContentScaleMode
   {
      /// <summary>
      /// The image is not scaled at all and will be rendered according to its original pixel dimensions,
      /// possibly overdrawing in both directions.
      /// </summary>
      None,
      /// <summary>
      /// The image will be stretched so that it completely fills out the containing control.
      /// This is the default value.
      /// </summary>
      Stretch,
      /// <summary>
      /// The image will retain its original aspect ratio, but scaled down (or up) accordingly
      /// to fill out as much of the control as possible, possibly leaving blank areas at an
      /// edge (top, bottom, left or right, respectively).
      /// </summary>
      AspectFit,
      /// <summary>
      /// The image will retain its original aspect ratio, but scaled down (or up) accordingly
      /// to fill out the complete control, possibly overdrawing at the top (or bottom, respectively).
      /// </summary>
      AspectStretch,
      /// <summary>
      /// The image will retain its original aspect ratio, but scaled up (or down) to cover the
      /// full width of the control, possibly overdrawing in the y-direction.
      /// </summary>
      FitWidth,
      /// <summary>
      /// The image will retain its original aspect ratio, but scaled up (or down) to cover the
      /// full height of the control, possibly overdrawing in the x-direction.
      /// </summary>
      FitHeight,
   }

   public enum UIImageContentGravity
   {
      TopLeft,
      TopCenter,
      TopRight,
      CenterLeft,
      Center,
      CenterRight,
      BottomLeft,
      BottomCenter,
      BottomRight
   }

   public class UIImage : UIBaseComponent
   {
      private Texture image;
      public Texture Image
      {
         get { return image; }
         set { image = value; PerformLayout(); }
      }

      public UIImageContentScaleMode ImageScaleMode { get; set; }

      /// <summary>
      /// Gets or sets the UIImage content gravity. The image inside this control will be aligned according
      /// to this property. 
      /// Default is Center.
      /// </summary>
      public UIImageContentGravity ImageContentGravity { get; set; }

      public Vector2 ImageSize => Image != null ? new Vector2(Image.Width, Image.Height) : Vector2.Zero;

      public float AspectRatio => Image != null ? (float)Image.Width / (float)Image.Height : 1.0f;

      private bool resizeImageToFill;
      /// <summary>
      /// Indicates whether the UIImage control should resize itself to match the image size. This is only useful
      /// for <see cref="ImageScaleMode"/>=AspectFit,FitWidth,FitHeight. In these cases one UIImage dimension will be
      /// resized (shrunken / enlarged) to match the actual image dimensions.
      /// For other <see cref="ImageScaleMode"/>-values this property has no effect.
      /// 
      /// Default value is false.
      /// </summary>
      public bool ResizeImageToFill
      {
         get { return resizeImageToFill; }
         set { resizeImageToFill = value; PerformLayout(); }
      }

      public override Vector2 PreferredSize
      {
         get
         {
            Vector2 resultSize = base.PreferredSize;

            Vector2 tmp = Vector2.Zero;
            float width = 0;
            float height = 0;
            MeasurePositionAndSize(ref tmp, ref width, ref height);

            if (ResizeImageToFill == false || ImageScaleMode == UIImageContentScaleMode.None || ImageScaleMode == UIImageContentScaleMode.Stretch)
            {
               if (width > resultSize.X)
               {
                  resultSize.X = width;
               }
               if (height > resultSize.Y)
               {
                  resultSize.Y = height;
               }
            } else
            {
               resultSize.X = width;
               resultSize.Y = height;
            }

            return resultSize;
         }
      }

      public UIImage(IRenderConfiguration renderConfig, Texture setImage)
         : base(renderConfig)
      {
         resizeImageToFill = false;
         image = setImage;

         ImageContentGravity = UIImageContentGravity.Center;

         ImageScaleMode = UIImageContentScaleMode.Stretch;

         if (image != null)
         {
            Width = Image.Width;
            Height = Image.Height;
         }
      }

      protected override void Draw()
      {
         base.Draw();

         if (Image != null)
         {
            Vector2 screenPos = ScreenPosition;
            Vector2 screenPosOffset = Vector2.Zero;
            float renderWidth = 0;
            float renderHeight = 0;

            MeasurePositionAndSize(ref screenPosOffset, ref renderWidth, ref renderHeight);

            Image.RenderOnScreen(screenPos.X + screenPosOffset.X, screenPos.Y + screenPosOffset.Y,
                                 renderWidth, renderHeight,
                                 Color.FromNonPremultiplied(new Vector4(Vector3.One, CompositeAlpha)), CompositeRotation);
         }
      }

      protected override void InternalPerformLayout()
      {
         base.InternalPerformLayout();

         Vector2 prefSize = PreferredSize;
         Width = prefSize.X;
         Height = prefSize.Y;
      }

      private void MeasurePositionAndSize(ref Vector2 position, ref float width, ref float height)
      {
         float aspectRatio = AspectRatio;

         switch (ImageScaleMode)
         {
            case UIImageContentScaleMode.None:
               width = ImageSize.X;
               height = ImageSize.Y;
               break;

            case UIImageContentScaleMode.Stretch:
               width = Width;
               height = Height;
               break;

            case UIImageContentScaleMode.AspectFit:
               {
                  if (Width >= Height)
                  {
                     height = Height;
                     width = height / aspectRatio;
                  } else
                  {
                     width = Width;
                     height = width / aspectRatio;
                  }
               }
               break;

            case UIImageContentScaleMode.AspectStretch:
               {
                  if (Width >= Height)
                  {
                     height = Height;
                     width = height * aspectRatio;
                  } else
                  {
                     width = Width;
                     height = width * aspectRatio;
                  }
               }
               break;

            case UIImageContentScaleMode.FitWidth:
               {
                  width = Width;
                  height = width / aspectRatio;
               }
               break;

            case UIImageContentScaleMode.FitHeight:
               {
                  height = Height;
                  width = height * aspectRatio;
               }
               break;
         }

         if (ImageContentGravity == UIImageContentGravity.TopCenter ||
             ImageContentGravity == UIImageContentGravity.TopLeft ||
             ImageContentGravity == UIImageContentGravity.TopRight)
         {
            position.Y = 0;
         }

         if (ImageContentGravity == UIImageContentGravity.Center ||
             ImageContentGravity == UIImageContentGravity.CenterLeft ||
             ImageContentGravity == UIImageContentGravity.CenterRight)
         {
            position.Y = Height / 2 - height / 2;
         }

         if (ImageContentGravity == UIImageContentGravity.BottomCenter ||
             ImageContentGravity == UIImageContentGravity.BottomLeft ||
             ImageContentGravity == UIImageContentGravity.BottomRight)
         {
            position.Y = Height - height;
         }

         if (ImageContentGravity == UIImageContentGravity.TopLeft ||
             ImageContentGravity == UIImageContentGravity.CenterLeft ||
             ImageContentGravity == UIImageContentGravity.BottomLeft)
         {
            position.X = 0;
         }

         if (ImageContentGravity == UIImageContentGravity.TopCenter ||
             ImageContentGravity == UIImageContentGravity.Center ||
             ImageContentGravity == UIImageContentGravity.BottomCenter)
         {
            position.X = Width / 2 - width / 2;
         }

         if (ImageContentGravity == UIImageContentGravity.TopRight ||
             ImageContentGravity == UIImageContentGravity.CenterRight ||
             ImageContentGravity == UIImageContentGravity.BottomRight)
         {
            position.X = Width - width;
         }
      }
   }
}
