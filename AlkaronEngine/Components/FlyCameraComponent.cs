using System;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
    public class FlyCameraComponent : CameraComponent
    {
        private Vector2 startPos;

        private float yaw;
        private float pitch;
        private float roll;

        public FlyCameraComponent(Vector3 setCenter, Vector2 setScreenSize,
                               float setNearClip, float setFarClip)
           : base(setCenter, Vector3.Up, setCenter + Vector3.Forward, setScreenSize,
                  setNearClip, setFarClip)
        {
            yaw = 0;
            pitch = 0;
            roll = 0;
        }

        public override bool PointerDown(Vector2 position, Input.PointerType pointerType)
        {
            if (base.PointerDown(position, pointerType))
            {
                return true;
            }

            startPos = position;

            return true;
        }

        public override bool PointerMoved(Vector2 position)
        {
            if (base.PointerMoved(position))
            {
                return true;
            }

            Vector2 delta = position - startPos;

            yaw -= delta.X;
            pitch -= delta.Y;

            Matrix rotMat = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch), MathHelper.ToRadians(roll));
            Vector3 newPosition = Vector3.Transform(Vector3.Forward, rotMat);

            LookAt = Center + newPosition;

            return true;
        }

        public override bool PointerUp(Vector2 position, Input.PointerType pointerType)
        {
            return base.PointerUp(position, pointerType);
        }

        public override bool PointerWheelChanged(Vector2 position)
        {
            if (base.PointerWheelChanged(position))
            {
                return true;
            }

            Vector3 camVec = (LookAt - Center) * SpeedModifier;
            if (position.X < 0)
            {
                Center += camVec;
                LookAt += camVec;
            }
            else if (position.X > 0)
            {
                Center -= camVec;
                LookAt -= camVec;
            }

            return true;
        }
    }
}
