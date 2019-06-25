using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Input
{
    public delegate void PointerEvent(Vector2 position, PointerType pointerType, double deltaTime);
    public delegate void KeyEvent(Key key, double deltaTime);

    public class InputManager
    {
        private Vector2 prevMousePos;
        private InputSnapshot curInputSnapshot;
        private InputSnapshot prevInputSnapshot;
        private float prevWheelValue;

        public event PointerEvent OnPointerMoved;
        public event PointerEvent OnPointerPressed;
        public event PointerEvent OnPointerReleased;
        public event PointerEvent OnPointerWheelChanged;

        public event KeyEvent OnKeyPressed;
        public event KeyEvent OnKeyReleased;

        /*private IRenderConfiguration renderConfig;*/

        public Vector2 MousePosition { get; private set; }
        public Vector2 ScaledMousePosition { get; private set; }

        public InputManager()
        {
            /*if (setRenderConfig == null)
            {
               throw new ArgumentNullException(nameof(setRenderConfig));
            }

            renderConfig = setRenderConfig;*/

            prevWheelValue = 0.0f;
            prevMousePos = new Vector2(-1, -1);
        }

        private static MouseEvent GetMouseEventForButton(MouseButton mouseButton, IReadOnlyList<MouseEvent> mouseEvents)
        {
            return (from e in mouseEvents
                    where e.MouseButton == mouseButton
                    select e).FirstOrDefault();
        }

        public void UpdateInput(InputSnapshot snapshot, double deltaTime)
        {
            /*
            Vector2 ScaledOffset = renderConfig.ScaledOffset;
            Vector2 Scale = renderConfig.Scale;

            Vector2 scaledPosition = new Vector2((mousePos.X - ScaledOffset.X) / Scale.X,
               (mousePos.Y - ScaledOffset.Y) / Scale.Y);*/

            curInputSnapshot = snapshot;

            MousePosition = snapshot.MousePosition;
            ScaledMousePosition = snapshot.MousePosition;

            if (MousePosition != prevMousePos)
            {
                OnPointerMoved?.Invoke(ScaledMousePosition, PointerType.None, deltaTime);
                prevMousePos = MousePosition;
            }

            // Mouse buttons
            foreach (var mouseEvent in snapshot.MouseEvents)
            {
                switch (mouseEvent.MouseButton)
                {
                    case MouseButton.Left:
                        {
                            var prevEvent = GetMouseEventForButton(MouseButton.Left, prevInputSnapshot.MouseEvents);
                            if (mouseEvent.Down == true && prevEvent.Down == false)
                            {
                                OnPointerPressed?.Invoke(ScaledMousePosition, PointerType.LeftMouse, deltaTime);
                            }
                            if (mouseEvent.Down == false && prevEvent.Down == true)
                            {
                                OnPointerReleased?.Invoke(ScaledMousePosition, PointerType.LeftMouse, deltaTime);
                            }
                        }
                        break;

                    case MouseButton.Middle:
                        {
                            var prevEvent = GetMouseEventForButton(MouseButton.Middle, prevInputSnapshot.MouseEvents);
                            if (mouseEvent.Down == true && prevEvent.Down == false)
                            {
                                OnPointerPressed?.Invoke(ScaledMousePosition, PointerType.MiddleMouse, deltaTime);
                            }
                            if (mouseEvent.Down == false && prevEvent.Down == true)
                            {
                                OnPointerReleased?.Invoke(ScaledMousePosition, PointerType.MiddleMouse, deltaTime);
                            }
                        }
                        break;

                    case MouseButton.Right:
                        {
                            var prevEvent = GetMouseEventForButton(MouseButton.Right, prevInputSnapshot.MouseEvents);
                            if (mouseEvent.Down == true && prevEvent.Down == false)
                            {
                                OnPointerPressed?.Invoke(ScaledMousePosition, PointerType.RightMouse, deltaTime);
                            }
                            if (mouseEvent.Down == false && prevEvent.Down == true)
                            {
                                OnPointerReleased?.Invoke(ScaledMousePosition, PointerType.RightMouse, deltaTime);
                            }
                        }
                        break;
                }
            }

            // Mouse wheel            
            float newWheelValue = snapshot.WheelDelta;
            float curWheelDelta = newWheelValue - prevWheelValue;
            prevWheelValue = newWheelValue;
            if (curWheelDelta != 0)
            {
                OnPointerWheelChanged?.Invoke(new Vector2(curWheelDelta, 0), PointerType.Wheel, deltaTime);
            }

            // Handle keyboard events
            HandleKeyboardEvents(deltaTime);

            prevInputSnapshot = snapshot;
        }

        private static List<Key> GetPressedKeys(InputSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return new List<Key>();
            }

            return (from k in snapshot.KeyEvents
                    where k.Down
                    select k.Key).ToList();
        }

        private static bool IsKeyDown(InputSnapshot snapshot, Key key)
        {
            if (snapshot == null)
            {
                return false;
            }

            return (from k in snapshot.KeyEvents
                    where k.Key == key &&
                          k.Down == true
                    select k).FirstOrDefault().Down;
        }

        private void HandleKeyboardEvents(double deltaTime)
        {
            var curPressedKeys = GetPressedKeys(curInputSnapshot);
            var prevPressedKeys = GetPressedKeys(prevInputSnapshot);

            var newlyPressedKeys = new List<Key>();
            var noLongerPressedKeys = new List<Key>();
            var stillPressedKeys = new List<Key>();

            // First build lists of newly or still pressed keys
            for (int i = curPressedKeys.Count - 1; i >= 0; i--)
            {
                Key key = curPressedKeys[i];
                if (prevPressedKeys.Contains(key))
                {
                    stillPressedKeys.Add(key);

                    curPressedKeys.RemoveAt(i);
                    prevPressedKeys.Remove(key);
                }
                else
                {
                    newlyPressedKeys.Add(key);
                }
            }

            // Remaining keys in prevPressedKeys are no longer pressed
            for (int i = 0; i < prevPressedKeys.Count; i++)
            {
                noLongerPressedKeys.Add(prevPressedKeys[i]);
            }

            if (OnKeyPressed != null)
            {
                for (int i = 0; i < newlyPressedKeys.Count; i++)
                {
                    OnKeyPressed(newlyPressedKeys[i], deltaTime);
                }
            }
            if (OnKeyReleased != null)
            {
                for (int i = 0; i < noLongerPressedKeys.Count; i++)
                {
                    OnKeyReleased(noLongerPressedKeys[i], deltaTime);
                }
            }
        }

        public bool IsKeyPressed(Key key)
        {
            return IsKeyDown(curInputSnapshot, key);
        }

        public bool WasKeyPressed(Key key)
        {
            return IsKeyDown(prevInputSnapshot, key);
        }

        public bool IsAnyKeyPressed()
        {
            return GetPressedKeys(curInputSnapshot).Count > 0;
        }
    }
}

