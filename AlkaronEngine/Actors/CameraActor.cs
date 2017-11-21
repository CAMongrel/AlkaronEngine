using System;
using AlkaronEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AlkaronEngine.Actors
{
    public class CameraActor : BaseActor
    {
        public CameraActor()
        {
        }

        public virtual bool PointerDown(Vector2 position, PointerType pointerType)
        {
            return false;
        }

        public virtual bool PointerUp(Vector2 position, PointerType pointerType)
        {
            return false;
        }

        public virtual bool PointerMoved(Vector2 position)
        {
            return false;
        }

        public virtual bool PointerWheelChanged(Vector2 position)
        {
            return false;
        }

        public virtual bool OnKeyEvent(Keys key, KeyEventType eventType)
        {
            return false;
        }
    }
}
