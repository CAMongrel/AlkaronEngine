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

        private bool moveForward;
        private bool moveBackward;
        private bool strafeLeft;
        private bool strafeRight;

        public FlyCameraComponent(Vector3 setCenter, Vector2 setScreenSize,
                               float setNearClip, float setFarClip)
           : base(setCenter, Vector3.Up, setCenter + Vector3.Forward, setScreenSize,
                  setNearClip, setFarClip)
        {
            yaw = 0;
            pitch = 0;
            roll = 0;

            moveForward = moveBackward = strafeLeft = strafeRight = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Vector3 camVec = (LookAt - Center) * SpeedModifier * (float)gameTime.ElapsedGameTime.TotalSeconds;
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

        public override bool KeyPressed(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (key == Microsoft.Xna.Framework.Input.Keys.W)
            {
                moveForward = true;
                return true;
            }
            if (key == Microsoft.Xna.Framework.Input.Keys.S)
            {
                moveBackward = true;
                return true;
            }
            if (key == Microsoft.Xna.Framework.Input.Keys.A)
            {
                strafeLeft = true;
                return true;
            }
            if (key == Microsoft.Xna.Framework.Input.Keys.D)
            {
                strafeRight = true;
                return true;
            }
            if (key == Microsoft.Xna.Framework.Input.Keys.LeftShift)
            {
                SpeedModifier *= 3.0f;
                return true;
            }

            return false;
        }

        public override bool KeyReleased(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (key == Microsoft.Xna.Framework.Input.Keys.W)
            {
                moveForward = false;
                return true;
            }
            if (key == Microsoft.Xna.Framework.Input.Keys.S)
            {
                moveBackward = false;
                return true;
            }
            if (key == Microsoft.Xna.Framework.Input.Keys.A)
            {
                strafeLeft = false;
                return true;
            }
            if (key == Microsoft.Xna.Framework.Input.Keys.D)
            {
                strafeRight = false;
                return true;
            }
            if (key == Microsoft.Xna.Framework.Input.Keys.LeftShift)
            {
                SpeedModifier /= 3.0f;
                return true;
            }

            return false;
        }
    }
}
