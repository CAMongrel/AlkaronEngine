using System;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
    public class ArcBallCameraComponent : CameraComponent
    {
        private Vector2 startPos;

        private float yawRadians;
        private float pitchRadians;
        private float rollRadians;

        public ArcBallCameraComponent(  Vector3 setLookAt,
                                        float setDistance,
                                        float setYawDegrees, float setPitchDegrees, float setRollDegrees,
                                        Vector2 setScreenSize,
                                        float setNearClip, float setFarClip)
           : base(Vector3.Zero, Vector3.UnitY, setLookAt, setScreenSize,
                  setNearClip, setFarClip)
        {
            yawRadians = MathHelper.ToRadians(setYawDegrees);
            pitchRadians = MathHelper.ToRadians(setPitchDegrees);
            rollRadians = MathHelper.ToRadians(setRollDegrees);

            Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(yawRadians, pitchRadians, rollRadians);
            Vector3 backward = Vector3.Backward;

            Vector3 directionVec = Vector3.Transform(backward, quaternion);

            SetCenter(directionVec * setDistance, false);
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

            yawRadians -= MathHelper.ToRadians(delta.X);
            pitchRadians -= MathHelper.ToRadians(delta.Y);

            Vector3 camVector = Center - LookAt;
            float distance = camVector.Length();

            Matrix rotMat = Matrix.CreateFromYawPitchRoll(yawRadians, pitchRadians, rollRadians);
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
