using System;
using System.Numerics;
using AlkaronEngine.Input;
using Veldrid;

namespace AlkaronEngine.Components
{
    public enum CameraViewType
    {
        Perspective,
        Orthographic
    }

    public class CameraComponent : BaseComponent
    {
        public Vector3 UpVector { get; set; }
        public Vector3 LookAt { get; set; }

        public Vector2 ScreenSize { get; private set; }

        public float NearClip { get; private set; }
        public float FarClip { get; private set; }

        public Matrix4x4 ViewMatrix { get; private set; }
        public Matrix4x4 ProjectionMatrix { get; private set; }

        public float SpeedModifier { get; set; }

        public CameraViewType CameraViewType { get; private set; }

        public CameraComponent(Vector3 setCenter, Vector3 setUpVector,
                               Vector3 setLookAt,
                               Vector2 setScreenSize,
                               float setNearClip, float setFarClip,
                               CameraViewType setCameraViewType)
           : base(setCenter)
        {
            CameraViewType = setCameraViewType;
            SpeedModifier = 1.0f;

            UpVector = setUpVector;
            LookAt = setLookAt;

            NearClip = setNearClip;
            FarClip = setFarClip;

            ScreenSize = setScreenSize;

            switch (CameraViewType)
            {
                case CameraViewType.Perspective:
                    ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(BepuUtilities.MathHelper.ToRadians(60.0f),
                                                                           ScreenSize.X / ScreenSize.Y, NearClip, FarClip);
                    break;

                case CameraViewType.Orthographic:
                    ProjectionMatrix = Matrix4x4.CreateOrthographic(ScreenSize.X, ScreenSize.Y, NearClip, FarClip);
                    break;
            }
            
            ViewMatrix = Matrix4x4.CreateLookAt(Center, LookAt, UpVector);
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            UpdateMatrices();
        }

        public void Zoom(float absoluteDelta)
        {
            Vector3 camVector = Center - LookAt;
            float length = camVector.Length();// + absoluteDelta;

            float delta = length * 0.05f * absoluteDelta;

            length += delta;

            camVector = Vector3.Normalize(camVector);
            Center = LookAt + (camVector * length);
        }

        private void UpdateMatrices()
        {
            ViewMatrix = Matrix4x4.CreateLookAt(Center, LookAt, UpVector);
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(BepuUtilities.MathHelper.ToRadians(60.0f),
                                                                   ScreenSize.X / ScreenSize.Y, NearClip, FarClip);
        }

        public virtual bool PointerDown(Vector2 position, PointerType pointerType)
        {
            return false;
        }

        public virtual bool PointerUp(Vector2 position, PointerType pointerType)
        {
            return false;
        }

        public virtual bool PointerMoved(Vector2 position, double deltaTime)
        {
            return false;
        }

        public virtual bool PointerWheelChanged(Vector2 position, double deltaTime)
        {
            return false;
        }

        public virtual bool KeyPressed(Key key)
        {
            return false;
        }

        public virtual bool KeyReleased(Key key)
        {
            return false;
        }
    }
}
