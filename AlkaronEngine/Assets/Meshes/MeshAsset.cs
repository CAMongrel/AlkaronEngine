// Project: Hellspawn

#region Using directives
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
#endregion

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
        protected VertexBuffer vertexBuffer = null;
        /// <summary>
        /// Index buffer for the mesh.
        /// </summary>
        protected IndexBuffer indexBuffer = null;

        /// <summary>
        /// 32 bit indices
        /// </summary>
        protected uint[] objectIndices;

        public Material Material { get; set; }

        protected const float EPSILON = 0.000001f;

        /// <summary>
        /// Boundingsphere of the object
        /// </summary>
        public BoundingSphere BoundingSphere { get; protected set; }

        /// <summary>
        /// Integrated cameras for this mesh. Can be used for model viewers, etc.
        /// 
        /// Will be null, if there are no cameras
        /// </summary>
        public MeshCamera[] MeshCameras { get; protected set; }

        internal virtual void CreateBoundingSphere()
        {
            //
        } // class MeshAsset
    }
} // namespace HellspawnEngine.Assets.Meshes
