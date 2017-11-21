using System;
using AlkaronEngine.Components;
using AlkaronEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AlkaronEngine.Actors
{
    public class CameraActor : BaseActor
    {
        public CameraComponent CameraComponent { get; private set; } 

        public CameraActor(CameraComponent setCameraComponent)
        {
            CameraComponent = setCameraComponent;
            AttachedComponents.Add(CameraComponent);
        }

        public virtual bool PointerDown(Vector2 position, PointerType pointerType)
        {
            return CameraComponent?.PointerDown(position, pointerType) ?? false;
        }

        public virtual bool PointerUp(Vector2 position, PointerType pointerType)
        {
            return CameraComponent?.PointerUp(position, pointerType) ?? false;
        }

        public virtual bool PointerMoved(Vector2 position)
        {
            return CameraComponent?.PointerMoved(position) ?? false;
        }

        public virtual bool PointerWheelChanged(Vector2 position)
        {
            return CameraComponent?.PointerWheelChanged(position) ?? false;
        }

        public virtual bool OnKeyEvent(Keys key, KeyEventType eventType)
        {
            return CameraComponent?.OnKeyEvent(key, eventType) ?? false;
        }
    }
}
