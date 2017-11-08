using System;
using AlkaronEngine.Input;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Graphics3D.Components
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

      public void ZoomIn(float absoluteDelta)
      {
         Vector3 camVector = Center - LookAt;
         float length = camVector.Length() - absoluteDelta;
         camVector.Normalize();
         Center = LookAt + camVector * length;
      }

      public void ZoomOut(float absoluteDelta)
      {
         Vector3 camVector = Center - LookAt;
         float length = camVector.Length() + absoluteDelta;
         camVector.Normalize();
         Center = LookAt + camVector * length;
      }

      private void UpdateMatrices()
      {
         ViewMatrix = Matrix.CreateLookAt(Center, LookAt, UpVector);
         ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60.0f),
                                                                ScreenSize.X / ScreenSize.Y, NearClip, FarClip);
      }

      public virtual void PointerDown(Vector2 position, PointerType pointerType)
      {
      }

      public virtual void PointerUp(Vector2 position, PointerType pointerType)
      {
      }

      public virtual void PointerMoved(Vector2 position)
      {
      }

      public virtual void PointerWheelChanged(Vector2 position)
      {
         
      }
   }
}
