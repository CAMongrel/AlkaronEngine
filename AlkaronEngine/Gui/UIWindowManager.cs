using AlkaronEngine.Graphics3D;
using AlkaronEngine.Input;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Gui
{
    internal static class UIWindowManager
    {
        private static object lockObj = new object();

        private static List<UIWindow> modalWindowStack = new List<UIWindow>();
        private static List<UIWindow> windows = new List<UIWindow>();

        internal static UIWindow? ModalWindow => modalWindowStack.Count == 0 ? null : modalWindowStack[modalWindowStack.Count - 1];

        internal static UIBaseComponent? HoverComponent { get; private set; } = null;
        internal static UIBaseComponent? CapturedComponent { get; private set; } = null;
        internal static UIBaseComponent? CapturedKeyboardComponent { get; private set; } = null;

        static UIWindowManager()
        {
            //
        }

        internal static bool AddWindow(UIWindow window, bool isModal)
        {
            if (window == null ||
                windows.Contains(window))
            {
                return false;
            }

            windows.Add(window);

            if (isModal)
            {
                modalWindowStack.Add(window);
            }

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

            if (modalWindowStack.Contains(window))
            {
                modalWindowStack.Remove(window);
            }

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

        internal static void Update(double deltaTime)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].Update(deltaTime);
            }
        }

        internal static void Draw(RenderContext renderContext)
        {
            Render(renderContext);
        }

        private static void Render(RenderContext renderContext)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].Visible == false)
                {
                    continue;
                }

                windows[i].InternalRender(renderContext);
            }
        }

        internal static void CaptureComponent(UIBaseComponent component)
        {
            lock (lockObj)
            {
                CapturedComponent = component;
            }
        }

        internal static bool PointerDown(Vector2 position, PointerType pointerType, double deltaTime)
        {
            var modalWindow = ModalWindow;
            if (modalWindow != null)
            {
                Vector2 localPosition = new Vector2(position.X - modalWindow.RelativeX, position.Y - modalWindow.RelativeY);
                if (modalWindow.HitTest(localPosition) == false)
                {
                    return false;
                }
                return modalWindow.PointerDown(localPosition, pointerType, deltaTime);
            }

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                Vector2 localPosition = new Vector2(position.X - windows[i].RelativeX, position.Y - windows[i].RelativeY);
                if (windows[i].HitTest(localPosition) == false)
                {
                    continue;
                }

                if (windows[i].PointerDown(localPosition, pointerType, deltaTime))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool PointerUp(Vector2 position, PointerType pointerType, double deltaTime)
        {
            lock (lockObj)
            {
                if (CapturedComponent != null)
                {
                    Vector2 relPosition = position - CapturedComponent.ScreenPosition;
                    return CapturedComponent.PointerUp(relPosition, pointerType, deltaTime);
                }
            }

            var modalWindow = ModalWindow;
            if (modalWindow != null)
            {
                Vector2 localPosition = new Vector2(position.X - modalWindow.RelativeX, position.Y - modalWindow.RelativeY);
                if (modalWindow.HitTest(localPosition) == false)
                {
                    return false;
                }
                return modalWindow.PointerUp(localPosition, pointerType, deltaTime);
            }

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                Vector2 localPosition = new Vector2(position.X - windows[i].RelativeX, position.Y - windows[i].RelativeY);
                if (windows[i].HitTest(localPosition) == false)
                {
                    continue;
                }

                if (windows[i].PointerUp(localPosition, pointerType, deltaTime))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool PointerMoved(Vector2 position, double deltaTime)
        {
            lock (lockObj)
            {
                if (CapturedComponent != null)
                {
                    Vector2 relPosition = position - CapturedComponent.ScreenPosition;
                    return CapturedComponent.PointerMoved(relPosition, deltaTime);
                }
            }

            var modalWindow = ModalWindow;
            if (modalWindow != null)
            {
                Vector2 localPosition = new Vector2(position.X - modalWindow.RelativeX, position.Y - modalWindow.RelativeY);
                if (modalWindow.HitTest(localPosition) == false)
                {
                    return false;
                }
                return modalWindow.PointerMoved(localPosition, deltaTime);
            }

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                Vector2 localPosition = new Vector2(position.X - windows[i].RelativeX, position.Y - windows[i].RelativeY);
                if (windows[i].HitTest(localPosition) == false)
                {
                    continue;
                }

                if (windows[i].PointerMoved(localPosition, deltaTime))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool PointerWheelChanged(Vector2 deltaValue, double deltaTime)
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

        internal static bool KeyReleased(Key key, double deltaTime)
        {
            lock (lockObj)
            {
                return CapturedKeyboardComponent?.KeyReleased(key, deltaTime) ?? false;
            }
        }

        internal static bool KeyPressed(Key key, double deltaTime)
        {
            lock (lockObj)
            {
                return CapturedKeyboardComponent?.KeyPressed(key, deltaTime) ?? false;
            }
        }
    }
}
