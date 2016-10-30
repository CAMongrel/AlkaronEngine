using AlkaronEngine.Gui;
using AlkaronEngine.Input;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Scene
{
   public class BaseScene
   {
      public static Color StandardBackgroundColor = new Color(0x7F, 0x00, 0x00);

      public MouseCursor MouseCursor { get; set; }

      public BaseScene()
      {
         
      }

      public virtual Color BackgroundColor
      {
         get
         {
            return StandardBackgroundColor;
         }
      }

      public virtual void Init()
      {
         InitUI();
      }

      protected virtual void InitUI()
      {
         //
      }

      public virtual void Close()
      {
         UIWindowManager.Clear();
      }

      public virtual void Update(GameTime gameTime)
      {
         UIWindowManager.Update(gameTime);
      }

      public virtual void Draw(GameTime gameTime)
      {
         UIWindowManager.Draw(gameTime);

         MouseCursor?.Render(gameTime);
      }

      public virtual void PointerDown(Vector2 position, PointerType pointerType)
      {
      }

      public virtual void PointerUp(Vector2 position, PointerType pointerType)
      {
      }

      public virtual void PointerMoved(Vector2 position)
      {
      }
   }
}
