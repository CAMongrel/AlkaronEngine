using System;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Graphics3D.Components
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

         Vector3 camVector = Center - LookAt;
         float distance = camVector.Length();

         Matrix rotMat = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch), MathHelper.ToRadians(roll));
         Vector3 newPosition = Vector3.Transform(Vector3.Backward, rotMat);
         newPosition *= distance;
         newPosition += LookAt;

         Center = newPosition;
      }

      public override void PointerUp(Vector2 position, Input.PointerType pointerType)
      {
         base.PointerUp(position, pointerType);
      }

      public override void PointerWheelChanged(Vector2 position)
      {
         base.PointerWheelChanged(position);

         if (position.X < 0)
         {
            ZoomIn(1.0f);
         }
         else if (position.X > 0)
         {
            ZoomOut(1.0f);
         }
      }
   }
}
