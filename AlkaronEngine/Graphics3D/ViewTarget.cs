using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlkaronEngine.Graphics3D
{
    internal class ViewTarget
    {
        public Vector3 CameraLocation { get; private set; }
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }
        public BoundingFrustum CameraFrustum { get; private set; }

        public ViewTarget(Vector3 setCameraLocation,
            Matrix setViewMatrix, Matrix setProjectionMatrix)
        {
            CameraLocation = setCameraLocation;
            ViewMatrix = setViewMatrix;
            ProjectionMatrix = setProjectionMatrix;

            CameraFrustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
        }
    }
}
