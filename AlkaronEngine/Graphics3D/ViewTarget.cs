
using System.Numerics;
using Veldrid.Utilities;

namespace AlkaronEngine.Graphics3D
{
    internal class ViewTarget
    {
        public Vector3 CameraLocation { get; private set; }
        public Matrix4x4 ViewMatrix { get; private set; }
        public Matrix4x4 ProjectionMatrix { get; private set; }
        public BoundingFrustum CameraFrustum { get; private set; }

        public ViewTarget(Vector3 setCameraLocation,
            Matrix4x4 setViewMatrix, Matrix4x4 setProjectionMatrix)
        {
            CameraLocation = setCameraLocation;
            ViewMatrix = setViewMatrix;
            ProjectionMatrix = setProjectionMatrix;

            CameraFrustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
        }
    }
}
