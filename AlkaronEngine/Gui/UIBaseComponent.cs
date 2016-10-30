#region Using directives
using AlkaronEngine.Graphics;
using AlkaronEngine.Input;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace AlkaronEngine.Gui
{
   public enum TextAlignHorizontal
   {
      Center,
      Left,
      Right,
   }

   public delegate void OnPointerDownInside(Vector2 position);
   public delegate void OnPointerUpInside(Vector2 position);
   public delegate void OnPointerUpOutside(Vector2 position);

   public abstract class UIBaseComponent : IDisposable
   {
      public float X;
      public float Y;
      public float Width;
      public float Height;

      public float Alpha;

      public Color BackgroundColor;

      public bool UserInteractionEnabled { get; set; }
      public bool Visible { get; set; }

      public float CompositeAlpha
      {
         get
         {
            if (ParentComponent == null)
               return Alpha;

            return Alpha * ParentComponent.CompositeAlpha;
         }
      }

      public Vector2 ScreenPosition
      {
         get
         {
            if (ParentComponent == null)
               return new Vector2(X, Y);

            Vector2 parentScreenPos = ParentComponent.ScreenPosition;
            return new Vector2(X + parentScreenPos.X, Y + parentScreenPos.Y);
         }
      }

      //internal UIBaseComponentList Components;
      private List<UIBaseComponent> components;

      internal UIBaseComponent ParentComponent { get; private set; }

      protected IRenderConfiguration renderConfig;

      public UIBaseComponent(IRenderConfiguration setRenderConfig)
      {
         if (setRenderConfig == null)
         {
            throw new ArgumentNullException(nameof(setRenderConfig));
         }
         renderConfig = setRenderConfig;

         BackgroundColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
         UserInteractionEnabled = true;
         Visible = true;
         Alpha = 1.0f;
         ParentComponent = null;
         components = new List<UIBaseComponent>();
         //Components = new UIBaseComponentList(components);
      }

      public void AddComponent(UIBaseComponent newComp)
      {
         if (newComp == null)
            return;

         if (newComp.ParentComponent != null)
            newComp.ParentComponent.RemoveComponent(newComp);

         newComp.ParentComponent = this;
         components.Add(newComp);
      }

      public void InsertComponent(UIBaseComponent newComp, int atPosition)
      {
         if (newComp == null)
            return;

         if (newComp.ParentComponent != null)
            newComp.ParentComponent.RemoveComponent(newComp);

         newComp.ParentComponent = this;

         components.Insert(atPosition, newComp);
      }

      public void RemoveComponent(UIBaseComponent comp)
      {
         if (comp == null)
            return;

         if (components.Contains(comp))
            components.Remove(comp);
         comp.ParentComponent = null;
      }

      public void ClearComponents()
      {
         components.Clear();
      }

      public void CenterOnScreen()
      {
         Vector2 OriginalAppSize = renderConfig.ScreenSize;

         X = (int)(OriginalAppSize.X / 2.0f - Width / 2.0f);
         Y = (int)(OriginalAppSize.Y / 2.0f - Height / 2.0f);
      }

      public void CenterInParent()
      {
         if (ParentComponent == null)
            return;

         X = ParentComponent.Width / 2 - Width / 2;
         Y = ParentComponent.Height / 2 - Height / 2;
      }

      public void CenterXInParent()
      {
         if (ParentComponent == null)
            return;

         X = ParentComponent.Width / 2 - Width / 2;
      }

      public void CenterYInParent()
      {
         if (ParentComponent == null)
            return;

         Y = ParentComponent.Height / 2 - Height / 2;
      }

      public Vector2 ConvertGlobalToLocal(Vector2 globalCoords)
      {
         Vector2 result = globalCoords;
         UIBaseComponent comp = this;

         while (comp != null)
         {
            result.X -= comp.X;
            result.Y -= comp.Y;

            comp = comp.ParentComponent;
         }

         return result;
      }

      public Vector2 ConvertLocalToGlobal(Vector2 localCoords)
      {
         Vector2 result = localCoords;
         UIBaseComponent comp = this;

         while (comp != null)
         {
            result.X += comp.X;
            result.Y += comp.Y;

            comp = comp.ParentComponent;
         }

         return result;
      }

      public virtual void Update(GameTime gameTime)
      {
         for (int i = 0; i < components.Count; i++)
         {
            Performance.Push(this.GetType() + ".components[" + i + "] - Update");
            components[i].Update(gameTime);
            Performance.Pop();
         }
      }

      internal void InternalRender()
      {
         // Call actual draw method
         Draw();

         // Iterate through child components
         for (int i = 0; i < components.Count; i++)
         {
            if (components[i].Visible == false)
               continue;

            components[i].InternalRender();
         }
      }

      public virtual void Draw()
      {
         // Draw background
         if (BackgroundColor.A > 0)
         {
            Texture.SingleWhite?.RenderOnScreen(ScreenPosition.X, ScreenPosition.Y, this.Width, this.Height, new Color(BackgroundColor, Alpha));
         }
      }

      public virtual bool PointerDown(Vector2 position, PointerType pointerType)
      {
         if (UserInteractionEnabled == false)
            return false;

         Vector2 relPosition = new Vector2(position.X - X, position.Y - Y);
         for (int i = components.Count - 1; i >= 0; i--)
         {
            Vector2 screenPos = components[i].ScreenPosition;
            if (!WinWarCS.Util.MathHelper.InsideRect(position, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)components[i].Width, (int)components[i].Height)))
               continue;

            if (components[i].PointerDown(relPosition, pointerType))
               return true;
         }

         return true;
      }

      public virtual bool PointerUp(Vector2 position, PointerType pointerType)
      {
         if (UserInteractionEnabled == false)
            return false;

         Vector2 relPosition = new Vector2(position.X - X, position.Y - Y);
         for (int i = components.Count - 1; i >= 0; i--)
         {
            Vector2 screenPos = components[i].ScreenPosition;
            if (!WinWarCS.Util.MathHelper.InsideRect(position, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)components[i].Width, (int)components[i].Height)))
               continue;

            if (components[i].PointerUp(relPosition, pointerType))
               return true;
         }

         return true;
      }

      public virtual bool PointerMoved(Vector2 position)
      {
         if (UserInteractionEnabled == false)
            return false;

         Vector2 relPosition = new Vector2(position.X - X, position.Y - Y);
         for (int i = components.Count - 1; i >= 0; i--)
         {
            Vector2 screenPos = components[i].ScreenPosition;
            if (!WinWarCS.Util.MathHelper.InsideRect(position, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)components[i].Width, (int)components[i].Height)))
               continue;

            if (components[i].PointerMoved(relPosition))
               return true;
         }

         return true;
      }

      public virtual void Dispose()
      {
         //
      }
   }
}
