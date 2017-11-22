using System;
using Microsoft.Xna.Framework;
using AlkaronEngine.Graphics2D;

namespace AlkaronEngine.Gui
{
    public class Cursor
    {
        public ushort HotSpotX { get; set; }
        public ushort HotSpotY { get; set; }
        public Texture Texture { get; set; }

        public Cursor(Texture setTexture, ushort setHotSpotX = 0, ushort setHotSpotY = 0)
        {
            HotSpotX = setHotSpotX;
            HotSpotY = setHotSpotY;
            Texture = setTexture;
        }
    }

    public class MouseCursor
    {
        public bool IsVisible { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 Hotspot
        {
            get
            {
                if (CurrentCursor == null)
                    return Vector2.Zero;

                return new Vector2(CurrentCursor.HotSpotX, CurrentCursor.HotSpotY);
            }
        }

        public Cursor CurrentCursor { get; set; }

        private IRenderConfiguration renderConfig;

        public MouseCursor(IRenderConfiguration setRenderConfig, Cursor setCurrentCursor)
        {
            if (setRenderConfig == null)
            {
                throw new ArgumentNullException(nameof(setRenderConfig));
            }
            renderConfig = setRenderConfig;

            Position = Vector2.Zero;
            IsVisible = true;
            CurrentCursor = setCurrentCursor;
        }

        public virtual void Render()
        {
            if (!IsVisible)
            {
                return;
            }

            if (CurrentCursor == null)
            {
                return;
            }

            Vector2 hotSpotOffset = Hotspot;
            CurrentCursor.Texture.RenderOnScreen(Position.X - hotSpotOffset.X, Position.Y - hotSpotOffset.Y);
        }
    }
}

