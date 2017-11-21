using System;
using AlkaronEngine.Actors;
using AlkaronEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AlkaronEngine.Controllers
{
    public class BaseController
    {
        public BaseActor PossessedActor { get; private set; }

        public BaseController()
        {
        }

        public void Possess(BaseActor actor)
        {
            PossessedActor = actor;
        }

        public virtual bool PointerDown(Vector2 position, PointerType pointerType)
        {
            // Base implementation does nothing
            return false;
        }

        public virtual bool PointerUp(Vector2 position, PointerType pointerType)
        {
            // Base implementation does nothing
            return false;
        }

        public virtual bool PointerMoved(Vector2 position)
        {
            // Base implementation does nothing
            return false;
        }

        public virtual bool PointerWheelChanged(Vector2 position)
        {
            // Base implementation does nothing
            return false;
        }

        public virtual bool OnKeyEvent(Keys key, KeyEventType eventType)
        {
            // Base implementation does nothing
            return false;
        }
    }
}
