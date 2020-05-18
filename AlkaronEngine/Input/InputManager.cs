using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Input
{
    public delegate void PointerEvent(Vector2 position, PointerType pointerType, double deltaTime);
    public delegate void KeyEvent(Key key, double deltaTime);

    class LocalInputSnapshot
    {
        internal Veldrid.KeyEvent[] KeyEvents;
        internal Veldrid.MouseEvent[] MouseEvents;
        internal char[] KeyCharPresses;
        internal Vector2 MousePosition;
        internal float WheelDelta;

        public static LocalInputSnapshot Create(InputSnapshot snapshot)
        {
            var localInputSnapshot = new LocalInputSnapshot();
            localInputSnapshot.KeyEvents = new Veldrid.KeyEvent[snapshot.KeyEvents.Count];
            for (int i = 0; i < localInputSnapshot.KeyEvents.Length; i++)
            {
                localInputSnapshot.KeyEvents[i] = snapshot.KeyEvents[i];
            }
            localInputSnapshot.MouseEvents = new MouseEvent[snapshot.MouseEvents.Count];
            for (int i = 0; i < localInputSnapshot.MouseEvents.Length; i++)
            {
                localInputSnapshot.MouseEvents[i] = snapshot.MouseEvents[i];
            }
            localInputSnapshot.KeyCharPresses = new char[snapshot.KeyCharPresses.Count];
            for (int i = 0; i < localInputSnapshot.KeyCharPresses.Length; i++)
            {
                localInputSnapshot.KeyCharPresses[i] = snapshot.KeyCharPresses[i];
            }
            localInputSnapshot.MousePosition = snapshot.MousePosition;
            localInputSnapshot.WheelDelta = snapshot.WheelDelta;
            return localInputSnapshot;
        }
    }

    public class InputManager
    {
        private Vector2 prevMousePos;
        private bool[] mouseDown;
        private LocalInputSnapshot curInputSnapshot;
        private LocalInputSnapshot prevInputSnapshot;

        public event PointerEvent OnPointerMoved;
        public event PointerEvent OnPointerPressed;
        public event PointerEvent OnPointerReleased;
        public event PointerEvent OnPointerWheelChanged;

        public event KeyEvent OnKeyPressed;
        public event KeyEvent OnKeyReleased;

        public Vector2 MousePosition { get; private set; }
        public Vector2 ScaledMousePosition { get; private set; }

        public InputManager()
        {
            prevMousePos = new Vector2(-1, -1);
            mouseDown = new bool[Enum.GetValues(typeof(MouseButton)).Length];
        }

        private static MouseEvent GetMouseEventForButton(MouseButton mouseButton, IReadOnlyList<MouseEvent> mouseEvents)
        {
            return (from e in mouseEvents
                    where e.MouseButton == mouseButton
                    select e).FirstOrDefault();
        }

        public void UpdateInput(InputSnapshot snapshot, double deltaTime)
        {
            curInputSnapshot = LocalInputSnapshot.Create(snapshot);

            MousePosition = curInputSnapshot.MousePosition;
            ScaledMousePosition = curInputSnapshot.MousePosition;

            if (MousePosition != prevMousePos)
            {
                OnPointerMoved?.Invoke(ScaledMousePosition, PointerType.None, deltaTime);
                prevMousePos = MousePosition;
            }

            // Mouse buttons
            foreach (var mouseEvent in curInputSnapshot.MouseEvents)
            {
                var preDown = mouseDown[(int)mouseEvent.MouseButton];
                mouseDown[(int)mouseEvent.MouseButton] = mouseEvent.Down;
                var curDown = mouseEvent.Down;

                switch (mouseEvent.MouseButton)
                {
                    case MouseButton.Left:
                        {
                            if (curDown == true && preDown == false)
                            {
                                OnPointerPressed?.Invoke(ScaledMousePosition, PointerType.LeftMouse, deltaTime);
                            }
                            if (curDown == false && preDown == true)
                            {
                                OnPointerReleased?.Invoke(ScaledMousePosition, PointerType.LeftMouse, deltaTime);
                            }
                        }
                        break;

                    case MouseButton.Middle:
                        {
                            if (curDown == true && preDown == false)
                            {
                                OnPointerPressed?.Invoke(ScaledMousePosition, PointerType.MiddleMouse, deltaTime);
                            }
                            if (curDown == false && preDown == true)
                            {
                                OnPointerReleased?.Invoke(ScaledMousePosition, PointerType.MiddleMouse, deltaTime);
                            }
                        }
                        break;

                    case MouseButton.Right:
                        {
                            if (curDown == true && preDown == false)
                            {
                                OnPointerPressed?.Invoke(ScaledMousePosition, PointerType.RightMouse, deltaTime);
                            }
                            if (curDown == false && preDown == true)
                            {
                                OnPointerReleased?.Invoke(ScaledMousePosition, PointerType.RightMouse, deltaTime);
                            }
                        }
                        break;
                }
            }

            // Mouse wheel
            float curWheelDelta = curInputSnapshot.WheelDelta;
            if (curWheelDelta != 0)
            {
                OnPointerWheelChanged?.Invoke(new Vector2(curWheelDelta, 0), PointerType.Wheel, deltaTime);
            }

            // Handle keyboard events
            HandleKeyboardEvents(deltaTime);

            prevInputSnapshot = curInputSnapshot;
        }

        private static List<Key> GetPressedKeys(LocalInputSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return new List<Key>();
            }
            
            return (from k in snapshot.KeyEvents
                    where k.Down
                    select k.Key).ToList();
        }

        private static bool IsKeyDown(LocalInputSnapshot snapshot, Key key)
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

        public static char GetCharForKey(Key key)
        {
            if (key >= Key.A && key <= Key.Z)
            {
                return (char)(key - 18);
            }
            if (key >= Key.Number0 && key <= Key.Number9)
            {
                return (char)(key - 61);
            }

            return (char)0;
        }
    }
}

