using AlkaronEngine.Assets.Materials;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace AlkaronEngine.Assets.Meshes
{
    public enum MeshCameraType
    {
        Perspective,
        Orthogonal
    }

    public class MeshCamera
    {
        public Vector3 Translation;

        public MeshCameraType CameraType;
    }

    /// <summary>
    /// Mesh asset
    /// </summary>
    public abstract class MeshAsset : Asset
    {
        /// <summary>
        /// Vertex buffer for the mesh.
        /// </summary>
        protected DeviceBuffer vertexBuffer = null;
        
        /// <summary>
        /// Index buffer for the mesh.
        /// </summary>
        protected DeviceBuffer indexBuffer = null;

        /// <summary>
        /// 32 bit indices
        /// </summary>
        protected uint[] objectIndices;

        /// <summary>
        /// Transform to the root node, must be applied during rendering.
        /// Defaults to Identity matrix
        /// </summary>
        public Matrix4x4 RootTransform = Matrix4x4.Identity;

        public IMaterial Material { get; set; }

        protected const float EPSILON = 0.000001f;

        /// <summary>
        /// Boundingsphere of the object
        /// </summary>
        public BoundingSphere BoundingSphere { get; protected set; }

        /// <summary>
        /// Integrated cameras for this mesh. Can be used for model viewers, etc.
        /// 
        /// Will be null if there are no cameras.
        /// </summary>
        public MeshCamera[] MeshCameras { get; protected set; }

        internal virtual void CreateBoundingSphere()
        {
            //
        } // class MeshAsset

        internal virtual void CalculateTangents()
        {
            //
        }
    }
} // namespace HellspawnEngine.Assets.Meshes
