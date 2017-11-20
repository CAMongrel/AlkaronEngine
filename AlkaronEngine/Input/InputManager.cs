using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using AlkaronEngine.Graphics2D;

namespace AlkaronEngine.Input
{
    public delegate void KeyEvent(Keys key, KeyEventType eventType);
    public delegate void PointerEvent(Vector2 position, PointerType pointerType);

    public class InputManager
    {
        private Vector2 prevMousePos;
        private MouseState prevMouseState;
        private int prevWheelValue;

        private KeyboardState prevKeyboardState;

        public event PointerEvent OnPointerMoved;
        public event PointerEvent OnPointerPressed;
        public event PointerEvent OnPointerReleased;
        public event PointerEvent OnPointerWheelChanged;

        public event KeyEvent OnKeyEvent;

        private IRenderConfiguration renderConfig;

        public Vector2 MousePosition { get; private set; }
        public Vector2 ScaledMousePosition { get; private set; }

        public InputManager(IRenderConfiguration setRenderConfig)
        {
            if (setRenderConfig == null)
            {
                throw new ArgumentNullException(nameof(setRenderConfig));
            }

            renderConfig = setRenderConfig;
            prevMousePos = new Vector2(-1, -1);

            MouseState mouseState = Mouse.GetState();
            prevWheelValue = mouseState.ScrollWheelValue;
        }

        public void UpdateInput(GameTime gameTime)
        {
            UpdateMouseState();
            UpdateKeyboardState();
        }

        private void UpdateKeyboardState()
        {
            KeyboardState keybState = Keyboard.GetState();

            Keys[] prevPressedKeys = prevKeyboardState.GetPressedKeys();

            Keys[] curPressedKeys = keybState.GetPressedKeys();

            for (int i = 0; i < curPressedKeys.Length; i++)
            {
                if (prevKeyboardState.IsKeyDown(curPressedKeys[i]) == false)
                {
                    // Key press event
                    OnKeyEvent?.Invoke(curPressedKeys[i], KeyEventType.Pressed);
                }
                else
                {
                    // Key held down
                    OnKeyEvent?.Invoke(curPressedKeys[i], KeyEventType.Pressed);
                }
            }

            for (int i = 0; i < prevPressedKeys.Length; i++)
            {
                if (keybState.IsKeyUp(prevPressedKeys[i]) == true)
                {
                    // Key released event
                    OnKeyEvent?.Invoke(prevPressedKeys[i], KeyEventType.Released);
                }
            }

            prevKeyboardState = keybState;
        }

        private void UpdateMouseState()
        {
            MouseState mouseState = Mouse.GetState();

            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

            Vector2 ScaledOffset = renderConfig.ScaledOffset;
            Vector2 Scale = renderConfig.Scale;

            Vector2 scaledPosition = new Vector2((mousePos.X - ScaledOffset.X) / Scale.X,
               (mousePos.Y - ScaledOffset.Y) / Scale.Y);

            MousePosition = mousePos;
            ScaledMousePosition = scaledPosition;

            if (mousePos != prevMousePos)
            {
                OnPointerMoved?.Invoke(scaledPosition, PointerType.None);
                prevMousePos = mousePos;
            }

            // Left mouse button
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                OnPointerPressed?.Invoke(scaledPosition, PointerType.LeftMouse);
            }
            if (mouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                OnPointerReleased?.Invoke(scaledPosition, PointerType.LeftMouse);
            }

            // Middle mouse button
            if (mouseState.MiddleButton == ButtonState.Pressed && prevMouseState.MiddleButton == ButtonState.Released)
            {
                OnPointerPressed?.Invoke(scaledPosition, PointerType.MiddleMouse);
            }
            if (mouseState.MiddleButton == ButtonState.Released && prevMouseState.MiddleButton == ButtonState.Pressed)
            {
                OnPointerReleased?.Invoke(scaledPosition, PointerType.MiddleMouse);
            }

            // Right mouse button
            if (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released)
            {
                OnPointerPressed?.Invoke(scaledPosition, PointerType.RightMouse);
            }
            if (mouseState.RightButton == ButtonState.Released && prevMouseState.RightButton == ButtonState.Pressed)
            {
                OnPointerReleased?.Invoke(scaledPosition, PointerType.RightMouse);
            }

            // Mouse wheel
            int newWheelValue = mouseState.ScrollWheelValue;
            int curWheelDelta = newWheelValue - prevWheelValue;
            prevWheelValue = newWheelValue;
            if (curWheelDelta != 0)
            {
                OnPointerWheelChanged?.Invoke(new Vector2(curWheelDelta, 0), PointerType.Wheel);
            }

            prevMouseState = mouseState;
        }
    }
}

