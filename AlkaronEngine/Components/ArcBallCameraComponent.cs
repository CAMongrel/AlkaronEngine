using System;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
    public class ArcBallCameraComponent : CameraComponent
    {
        private Vector2 startPos;

        private float yaw;
        private float pitch;
        private float roll;

        public ArcBallCameraComponent(Vector3 setCenter, Vector3 setUpVector,
                               Vector3 setLookAt,
                               Vector2 setScreenSize,
                               float setNearClip, float setFarClip)
           : base(setCenter, setUpVector, setLookAt, setScreenSize,
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

            Vector3 camVector = Center - LookAt;
            float distance = camVector.Length();

            Matrix rotMat = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch), MathHelper.ToRadians(roll));
            Vector3 newPosition = Vector3.Transform(Vector3.Backward, rotMat);
            newPosition *= distance;
            newPosition += LookAt;

            Center = newPosition;

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

            if (position.X < 0)
            {
                Zoom(-1.0f * SpeedModifier);
            }
            else if (position.X > 0)
            {
                Zoom(1.0f * SpeedModifier);
            }

            return true;
        }
    }
}
