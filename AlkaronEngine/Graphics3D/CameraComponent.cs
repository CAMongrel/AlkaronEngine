using System;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Graphics3D
{
   public class CameraComponent : BaseComponent
   {
      public Vector3 UpVector { get; set; }
      public Vector3 LookAt { get; set; }

      public Vector2 ScreenSize { get; private set; }

      public float NearClip { get; private set; }
      public float FarClip { get; private set; }

      public Matrix ViewMatrix { get; private set; }
      public Matrix ProjectionMatrix { get; private set; }

      public CameraComponent(Vector3 setCenter, Vector3 setUpVector, 
                             Vector3 setLookAt,
                             Vector2 setScreenSize,
                             float setNearClip, float setFarClip)
         : base(setCenter)
      {
         UpVector = setUpVector;
         LookAt = setLookAt;

         NearClip = setNearClip;
         FarClip = setFarClip;

         ScreenSize = setScreenSize;

         ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60.0f), 
                                                                ScreenSize.X / ScreenSize.Y, NearClip, FarClip);
         ViewMatrix = Matrix.CreateLookAt(Center, LookAt, UpVector);
      }

      public override void Update(GameTime gameTime)
      {
         base.Update(gameTime);

         UpdateMatrices();
      }

      private void UpdateMatrices()
      {
         ViewMatrix = Matrix.CreateLookAt(Center, LookAt, UpVector);
         ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60.0f),
                                                                ScreenSize.X / ScreenSize.Y, NearClip, FarClip);
      }
   }
}
