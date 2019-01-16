using AlkaronEngine.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlkaronEngine.Gui
{
    internal static class UIWindowManager
    {
        private static object lockObj = new object();

        private static List<UIWindow> windows;

        internal static UIBaseComponent HoverComponent { get; private set; }
        internal static UIBaseComponent CapturedComponent { get; private set; }
        internal static UIBaseComponent CapturedKeyboardComponent { get; private set; }

        static UIWindowManager()
        {
            windows = new List<UIWindow>();
        }

        internal static bool AddWindow(UIWindow window)
        {
            if (window == null ||
                windows.Contains(window))
            {
                return false;
            }

            windows.Add(window);
            return true;
        }

        internal static bool RemoveWindow(UIWindow window)
        {
            if (window == null ||
                windows.Contains(window) == false)
            {
                return false;
            }

            windows.Remove(window);
            return true;
        }

        internal static void Clear()
        {
            windows.Clear();
        }

        /// <summary>
        /// Calls PerformLayout for all windows
        /// </summary>
        internal static void PerformLayout()
        {
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].PerformLayout();
            }
        }

        internal static void Update(GameTime gameTime)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].Update(gameTime);
            }
        }

        internal static void Draw()
        {
            Render();
        }

        private static void Render()
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].Visible == false)
                {
                    continue;
                }

                windows[i].InternalRender();
            }
        }

        internal static void CaptureComponent(UIBaseComponent component)
        {
            lock (lockObj)
            {
                CapturedComponent = component;
            }
        }

        internal static bool PointerDown(Vector2 position, PointerType pointerType, GameTime gameTime)
        {
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                Vector2 localPosition = new Vector2(position.X - windows[i].RelativeX, position.Y - windows[i].RelativeY);
                if (windows[i].HitTest(localPosition) == false)
                {
                    continue;
                }

                if (windows[i].PointerDown(localPosition, pointerType, gameTime))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool PointerUp(Vector2 position, PointerType pointerType, GameTime gameTime)
        {
            lock (lockObj)
            {
                if (CapturedComponent != null)
                {
                    Vector2 relPosition = position - CapturedComponent.ScreenPosition;
                    return CapturedComponent.PointerUp(relPosition, pointerType, gameTime);
                }
            }

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                Vector2 localPosition = new Vector2(position.X - windows[i].RelativeX, position.Y - windows[i].RelativeY);
                if (windows[i].HitTest(localPosition) == false)
                {
                    continue;
                }

                if (windows[i].PointerUp(localPosition, pointerType, gameTime))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool PointerMoved(Vector2 position, GameTime gameTime)
        {
            lock (lockObj)
            {
                if (CapturedComponent != null)
                {
                    Vector2 relPosition = position - CapturedComponent.ScreenPosition;
                    return CapturedComponent.PointerMoved(relPosition, gameTime);
                }
            }

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                Vector2 localPosition = new Vector2(position.X - windows[i].RelativeX, position.Y - windows[i].RelativeY);
                if (windows[i].HitTest(localPosition) == false)
                {
                    continue;
                }

                if (windows[i].PointerMoved(localPosition, gameTime))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool PointerWheelChanged(Vector2 deltaValue, GameTime gameTime)
        {
            return false;
        }

        internal static void CaptureKeyboardFocus(UIBaseComponent component)
        {
            lock (lockObj)
            {
                CapturedKeyboardComponent = component;
            }
        }

        internal static bool KeyReleased(Microsoft.Xna.Framework.Input.Keys key, GameTime gameTime)
        {
            lock (lockObj)
            {
                return CapturedKeyboardComponent?.KeyReleased(key, gameTime) ?? false;
            }
        }

        internal static bool KeyPressed(Microsoft.Xna.Framework.Input.Keys key, GameTime gameTime)
        {
            lock (lockObj)
            {
                return CapturedKeyboardComponent?.KeyPressed(key, gameTime) ?? false;
            }
        }
    }
}
