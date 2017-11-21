using AlkaronEngine.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace AlkaronEngine.Gui
{
    internal static class UIWindowManager
    {
        private static object lockObj = new object();

        private static List<UIWindow> windows;

        internal static UIBaseComponent HoverComponent { get; private set; }
        internal static UIBaseComponent CapturedComponent { get; private set; }

        static UIWindowManager()
        {
            windows = new List<UIWindow>();
        }

        internal static void AddWindow(UIWindow window)
        {
            windows.Add(window);
        }

        internal static void RemoveWindow(UIWindow window)
        {
            if (window == null)
            {
                return;
            }

            if (windows.Contains(window))
            {
                windows.Remove(window);
                window.DidRemove();
            }
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

        internal static void Draw(GameTime gameTime)
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

        internal static bool PointerDown(Vector2 position, PointerType pointerType)
        {
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                if (windows[i].HitTest(position) == false)
                {
                    continue;
                }

                if (windows[i].PointerDown(position, pointerType))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool PointerUp(Vector2 position, PointerType pointerType)
        {
            lock (lockObj)
            {
                if (CapturedComponent != null)
                {
                    Vector2 relPosition = position - CapturedComponent.ScreenPosition;
                    return CapturedComponent.PointerUp(relPosition, pointerType);
                }
            }

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                if (windows[i].HitTest(position) == false)
                {
                    continue;
                }

                if (windows[i].PointerUp(position, pointerType))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool PointerMoved(Vector2 position)
        {
            lock (lockObj)
            {
                if (CapturedComponent != null)
                {
                    Vector2 relPosition = position - CapturedComponent.ScreenPosition;
                    return CapturedComponent.PointerMoved(relPosition);
                }
            }

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                if (windows[i].HitTest(position) == false)
                {
                    continue;
                }

                if (windows[i].PointerMoved(position))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool PointerWheelChanged(Vector2 position)
        {
            return false;
        }

        internal static bool KeyEvent(Keys key, KeyEventType eventType)
        {
            lock (lockObj)
            {
                if (CapturedComponent != null)
                {
                    return CapturedComponent.KeyEvent(key, eventType);
                }
            }

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                if (windows[i].KeyEvent(key, eventType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
