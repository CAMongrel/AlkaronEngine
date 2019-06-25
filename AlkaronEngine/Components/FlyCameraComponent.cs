using System.Numerics;
using Veldrid;

namespace AlkaronEngine.Components
{
    public class FlyCameraComponent : CameraComponent
    {
        private Vector2 startPos;

        private float yaw;
        private float pitch;
        private float roll;

        private bool moveForward;
        private bool moveBackward;
        private bool strafeLeft;
        private bool strafeRight;

        public FlyCameraComponent(Vector3 setCenter, Vector2 setScreenSize,
                               float setNearClip, float setFarClip)
           : base(setCenter, Vector3.UnitY, setCenter + Vector3.UnitZ, setScreenSize,
                  setNearClip, setFarClip)
        {
            yaw = 0;
            pitch = 0;
            roll = 0;

            moveForward = moveBackward = strafeLeft = strafeRight = false;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            Vector3 camVec = (LookAt - Center) * SpeedModifier * (float)deltaTime;
            if (moveForward)
            {
                Center += camVec;
                LookAt += camVec;
            }
            else if (moveBackward)
            {
                Center -= camVec;
                LookAt -= camVec;
            }
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

            Matrix4x4 rotMat = Matrix4x4.CreateFromYawPitchRoll(BepuUtilities.MathHelper.ToRadians(yaw), 
                BepuUtilities.MathHelper.ToRadians(pitch), BepuUtilities.MathHelper.ToRadians(roll));
            Vector3 newPosition = Vector3.Transform(Vector3.UnitZ, rotMat);

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

        public override bool KeyPressed(Key key)
        {
            if (key == Key.W)
            {
                moveForward = true;
                return true;
            }
            if (key == Key.S)
            {
                moveBackward = true;
                return true;
            }
            if (key == Key.A)
            {
                strafeLeft = true;
                return true;
            }
            if (key == Key.D)
            {
                strafeRight = true;
                return true;
            }
            if (key == Key.ShiftLeft)
            {
                SpeedModifier *= 3.0f;
                return true;
            }

            return false;
        }

        public override bool KeyReleased(Key key)
        {
            if (key == Key.W)
            {
                moveForward = false;
                return true;
            }
            if (key == Key.S)
            {
                moveBackward = false;
                return true;
            }
            if (key == Key.A)
            {
                strafeLeft = false;
                return true;
            }
            if (key == Key.D)
            {
                strafeRight = false;
                return true;
            }
            if (key == Key.ShiftLeft)
            {
                SpeedModifier /= 3.0f;
                return true;
            }

            return false;
        }
    }
}
