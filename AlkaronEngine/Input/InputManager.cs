using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Input
{
    public delegate void PointerEvent(Vector2 position, PointerType pointerType, double deltaTime);
    public delegate void KeyEvent(Key key, double deltaTime);

    public class InputManager
    {
        /*private Vector2 prevMousePos;
        private MouseState prevMouseState;
        private KeyboardState curKeyboardState;
        private KeyboardState prevKeyboardState;
        private int prevWheelValue;*/

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

            renderConfig = setRenderConfig;
            prevMousePos = new Vector2(-1, -1);

            MouseState mouseState = Mouse.GetState();
            prevWheelValue = mouseState.ScrollWheelValue;*/
        }

        public void UpdateInput(InputSnapshot snapshot, double gameTime)
        {
            /*MouseState mouseState = Mouse.GetState();

            curKeyboardState = Keyboard.GetState();

            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

            Vector2 ScaledOffset = renderConfig.ScaledOffset;
            Vector2 Scale = renderConfig.Scale;

            Vector2 scaledPosition = new Vector2((mousePos.X - ScaledOffset.X) / Scale.X,
               (mousePos.Y - ScaledOffset.Y) / Scale.Y);

            MousePosition = mousePos;
            ScaledMousePosition = scaledPosition;

            if (mousePos != prevMousePos)
            {
               OnPointerMoved?.Invoke(scaledPosition, PointerType.None, gameTime);
               prevMousePos = mousePos;
            }

            // Left mouse button
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
               OnPointerPressed?.Invoke(scaledPosition, PointerType.LeftMouse, gameTime);
            }
            if (mouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
               OnPointerReleased?.Invoke(scaledPosition, PointerType.LeftMouse, gameTime);
            }

            // Middle mouse button
            if (mouseState.MiddleButton == ButtonState.Pressed && prevMouseState.MiddleButton == ButtonState.Released)
            {
               OnPointerPressed?.Invoke(scaledPosition, PointerType.MiddleMouse, gameTime);
            }
            if (mouseState.MiddleButton == ButtonState.Released && prevMouseState.MiddleButton == ButtonState.Pressed)
            {
               OnPointerReleased?.Invoke(scaledPosition, PointerType.MiddleMouse, gameTime);
            }

            // Right mouse button
            if (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released)
            {
               OnPointerPressed?.Invoke(scaledPosition, PointerType.RightMouse, gameTime);
            }
            if (mouseState.RightButton == ButtonState.Released && prevMouseState.RightButton == ButtonState.Pressed)
            {
               OnPointerReleased?.Invoke(scaledPosition, PointerType.RightMouse, gameTime);
            }

            // Mouse wheel
            int newWheelValue = mouseState.ScrollWheelValue;
            int curWheelDelta = newWheelValue - prevWheelValue;
            prevWheelValue = newWheelValue;
            if (curWheelDelta != 0)
            {
               OnPointerWheelChanged?.Invoke(new Vector2(curWheelDelta, 0), PointerType.Wheel, gameTime);
            }

            // Handle keyboard events
            HandleKeyboardEvents(curKeyboardState, prevKeyboardState, gameTime);

            prevMouseState = mouseState;
            prevKeyboardState = curKeyboardState;*/
        }

        /*private void HandleKeyboardEvents(KeyboardState curState, KeyboardState prevState, GameTime gameTime)
        {
            var curPressedKeys = new List<Keys>(curState.GetPressedKeys());
            var prevPressedKeys = new List<Keys>(prevState.GetPressedKeys());

            var newlyPressedKeys = new List<Keys>();
            var noLongerPressedKeys = new List<Keys>();
            var stillPressedKeys = new List<Keys>();

            // First build lists of newly or still pressed keys
            for (int i = curPressedKeys.Count - 1; i >= 0; i--)
            {
                Keys key = curPressedKeys[i];
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
                    OnKeyPressed(newlyPressedKeys[i], gameTime);
                }
            }
            if (OnKeyReleased != null)
            {
                for (int i = 0; i < noLongerPressedKeys.Count; i++)
                {
                    OnKeyReleased(noLongerPressedKeys[i], gameTime);
                }
            }
        }
        */

        public bool IsKeyPressed(Key key)
        {
            return curKeyboardState.IsKeyDown(key);
        }

        public bool WasKeyPressed(Key key)
        {
            return prevKeyboardState.IsKeyDown(key);
        }

        public bool IsAnyKeyPressed()
        {
            return curKeyboardState.GetPressedKeys().Length > 0;
        }
    }
}

