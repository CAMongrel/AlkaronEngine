using System;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Graphics3D.Components
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

      public override void PointerDown(Vector2 position, Input.PointerType pointerType)
      {
         base.PointerDown(position, pointerType);

         startPos = position;
      }

      public override void PointerMoved(Vector2 position)
      {
         base.PointerMoved(position);

         Vector2 delta = position - startPos;

         yaw -= delta.X;
         pitch -= delta.Y;

         Matrix rotMat = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch), MathHelper.ToRadians(roll));
         Vector3 newPosition = Vector3.Transform(Vector3.Forward, rotMat);

         LookAt = Center + newPosition;
      }

      public override void PointerUp(Vector2 position, Input.PointerType pointerType)
      {
         base.PointerUp(position, pointerType);
      }

      public override void PointerWheelChanged(Vector2 position)
      {
         base.PointerWheelChanged(position);

         Vector3 camVec = LookAt - Center;
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
      }
   }
}
