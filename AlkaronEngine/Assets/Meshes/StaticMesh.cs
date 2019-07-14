using AlkaronEngine.Graphics;
using System;
using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace AlkaronEngine.Assets.Meshes
{
    public class StaticMesh : MeshAsset
    {
        protected override int MaxAssetVersion => 1;

        #region Members
        protected TangentVertex[] objectVertices;

        /// <summary>
        /// Number of vertices of this mesh
        /// </summary>
        public int NumberOfVertices
        {
            get
            {
                return objectVertices.Length;
            }
        }

        /// <summary>
        /// Number of faces of this mesh
        /// </summary>
        public int NumberOfFaces
        {
            get
            {
                return objectIndices.Length / 3;
            }
        }
        #endregion

        #region StaticMesh
        /// <summary>
        /// ctor
        /// </summary>
        public StaticMesh()
        {
            vertexBuffer = null;
            indexBuffer = null;
            AssetVersion = MaxAssetVersion;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Releases all XNA resources
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }
            if (indexBuffer != null)
            {
                indexBuffer.Dispose();
                indexBuffer = null;
            }
        }
        #endregion

        #region Deserialize
        /// <summary>
        /// Loads the static mesh from the compiled binary mesh
        /// </summary>
        internal override void Deserialize(BinaryReader reader, AssetSettings assetSettings)
        {
            base.Deserialize(reader, assetSettings);

            // Read mesh data
            int NumTriangles = reader.ReadInt32();
            int vertLen = reader.ReadInt32();
            objectVertices = new TangentVertex[vertLen];
            for (int i = 0; i < objectVertices.Length; i++)
            {
                objectVertices[i] = new TangentVertex(
                    new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle()),
                    new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle()),
                    new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle()),
                    new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle()));
            }

            int indexLen = reader.ReadInt32();
            objectIndices = new uint[indexLen];
            for (int i = 0; i < objectIndices.Length; i++)
            {
                objectIndices[i] = reader.ReadUInt32();
            }

            // Read BoundingSphere
            BoundingSphere = new BoundingSphere(
                new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle()),
                reader.ReadSingle());

            // Read MeshCameras
            int meshCameraCount = reader.ReadInt32();
            if (meshCameraCount == 0)
            {
                MeshCameras = null;
            }
            else
            {
                MeshCameras = new MeshCamera[meshCameraCount];
                for (int i = 0; i < meshCameraCount; i++)
                {
                    MeshCameras[i] = new MeshCamera();
                    MeshCameras[i].Translation = new Vector3(
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    MeshCameras[i].CameraType = (MeshCameraType)reader.ReadInt32();
                }
            }

            // Create XNA buffers and fill them
            try
            {
                BufferDescription vertexBufferDesc = new BufferDescription((uint)(TangentVertex.SizeInBytes * objectVertices.Length), BufferUsage.VertexBuffer);
                DeviceBuffer newVertexBuffer = assetSettings.GraphicsDevice.ResourceFactory.CreateBuffer(vertexBufferDesc);

                BufferDescription indexBufferDesc = new BufferDescription((uint)(sizeof(int) * objectIndices.Length), BufferUsage.IndexBuffer);
                DeviceBuffer newIndexBuffer = assetSettings.GraphicsDevice.ResourceFactory.CreateBuffer(indexBufferDesc);

                if (newVertexBuffer != null)
                {
                    if (vertexBuffer != null)
                    {
                        vertexBuffer.Dispose();
                        vertexBuffer = null;
                    }

                    vertexBuffer = newVertexBuffer;
                }
                if (newIndexBuffer != null)
                {
                    if (indexBuffer != null)
                    {
                        indexBuffer.Dispose();
                        indexBuffer = null;
                    }

                    indexBuffer = newIndexBuffer;
                }
            }
            catch
            {
                return;
            }

            assetSettings.GraphicsDevice.UpdateBuffer(vertexBuffer, 0, objectVertices);
            assetSettings.GraphicsDevice.UpdateBuffer(indexBuffer, 0, objectIndices);

            CreateBoundingSphere();
        }
        #endregion

        #region FromVertices
        /// <summary>
        /// Creates a static mesh directly from vertices (triangles only)
        /// </summary>
        public static StaticMesh FromVertices(Vector3[] vertices, GraphicsDevice graphicsDevice)
        {
            TangentVertex[] verts = new TangentVertex[vertices.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = new TangentVertex(
                    vertices[i], Vector2.Zero, new Vector3(0, 1, 0),
                    Vector3.Zero, Vector3.Zero);
            }

            return FromVertices(verts, graphicsDevice);
        }

        /// <summary>
        /// Creates a static mesh directly from tangent vertices (triangles only)
        /// </summary>
        public static StaticMesh FromVertices(TangentVertex[] vertices, GraphicsDevice graphicsDevice)
        {
            uint[] indices = new uint[vertices.Length];
            for (uint i = 0; i < vertices.Length; i++)
            {
                indices[i] = i;
            }

            return FromVertices(vertices, indices, graphicsDevice);
        }

        /// <summary>
        /// Creates a static mesh directly from vertices and indices
        /// </summary>
        public static StaticMesh FromVertices(TangentVertex[] vertices, uint[] indices, GraphicsDevice graphicsDevice)
        {
            StaticMesh mesh = new StaticMesh();

            mesh.objectVertices = vertices;
            mesh.objectIndices = indices;

            BufferDescription vertexBufferDesc = new BufferDescription((uint)(TangentVertex.SizeInBytes * mesh.objectVertices.Length), BufferUsage.VertexBuffer);
            mesh.vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(vertexBufferDesc);

            BufferDescription indexBufferDesc = new BufferDescription((uint)(sizeof(int) * mesh.objectIndices.Length), BufferUsage.IndexBuffer);
            mesh.indexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(indexBufferDesc);

            graphicsDevice.UpdateBuffer(mesh.vertexBuffer, 0, mesh.objectVertices);
            graphicsDevice.UpdateBuffer(mesh.indexBuffer, 0, mesh.objectIndices);

            mesh.CreateBoundingSphere();
            //mesh.CreateRuntimeCollisionData(CollisionType.Vertices);

            mesh.Name = mesh.GetType().ToString();

            return mesh;
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Saves the static mesh into its binary representation
        /// </summary>
        internal override void Serialize(BinaryWriter writer, AssetSettings assetSettings)
        {
            base.Serialize(writer, assetSettings);

            writer.Write(NumberOfFaces);
            writer.Write(objectVertices.Length);
            for (int i = 0; i < objectVertices.Length; i++)
            {
                writer.Write(objectVertices[i].Position.X);
                writer.Write(objectVertices[i].Position.Y);
                writer.Write(objectVertices[i].Position.Z);

                writer.Write(objectVertices[i].TexCoord.X);
                writer.Write(objectVertices[i].TexCoord.Y);

                writer.Write(objectVertices[i].Normal.X);
                writer.Write(objectVertices[i].Normal.Y);
                writer.Write(objectVertices[i].Normal.Z);

                writer.Write(objectVertices[i].Tangent.X);
                writer.Write(objectVertices[i].Tangent.Y);
                writer.Write(objectVertices[i].Tangent.Z);

                writer.Write(objectVertices[i].Bitangent.X);
                writer.Write(objectVertices[i].Bitangent.Y);
                writer.Write(objectVertices[i].Bitangent.Z);
            }

            writer.Write(objectIndices.Length);
            for (int i = 0; i < objectIndices.Length; i++)
            {
                writer.Write(objectIndices[i]);
            }

            // Write bounding sphere
            writer.Write(BoundingSphere.Center.X);
            writer.Write(BoundingSphere.Center.Y);
            writer.Write(BoundingSphere.Center.Z);
            writer.Write(BoundingSphere.Radius);

            // Write cameras
            if (MeshCameras != null)
            {
                writer.Write(MeshCameras.Length);
                for (int i = 0; i < MeshCameras.Length; i++)
                {
                    writer.Write(MeshCameras[i].Translation.X);
                    writer.Write(MeshCameras[i].Translation.Y);
                    writer.Write(MeshCameras[i].Translation.Z);
                    writer.Write((int)MeshCameras[i].CameraType);
                }
            }
            else
            {
                writer.Write((Int32)0);
            }
        }
        #endregion

        #region Intersect
        internal bool Intersect(Ray ray, out float distance, ref Vector3 scaling)
        {
            distance = 0;
            float u, v;
            for (int i = 0; i < objectIndices.Length; i += 3)
            {
                if (intersect_triangle(ray, objectIndices[i], objectIndices[i + 1],
                    objectIndices[i + 2], out distance, out u, out v, ref scaling))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region intersect_triangle
        bool intersect_triangle(Ray ray, uint vert0, uint vert1, uint vert2,
            out float t, out float u, out float v, ref Vector3 scaling)
        {
            Vector3 edge1, edge2, tvec, pvec, qvec;
            float det, inv_det;

            t = u = v = 0;

            // precache scaling of vert0
            Vector3 scaledVert0 = objectVertices[vert0].Position * scaling;
            // find vectors for two edges sharing vert0
            edge1 = (objectVertices[vert1].Position * scaling) - scaledVert0;
            edge2 = (objectVertices[vert2].Position * scaling) - scaledVert0;

            // begin calculating determinant - also used to calculate U parameter
            pvec = Vector3.Cross(ray.Direction, edge2);

            // if determinant is near zero, ray lies in plane of triangle
            det = Vector3.Dot(edge1, pvec);

            // the non-culling branch
            if (det > -EPSILON && det < EPSILON)
                return false;
            inv_det = 1.0f / det;

            // calculate distance from vert0 to ray origin
            tvec = ray.Origin - scaledVert0;

            // calculate U parameter and test bounds
            u = Vector3.Dot(tvec, pvec) * inv_det;
            if (u < 0.0 || u > 1.0)
                return false;

            // prepare to test V parameter 
            qvec = Vector3.Cross(tvec, edge1);

            // calculate V parameter and test bounds
            v = Vector3.Dot(ray.Direction, qvec) * inv_det;
            if (v < 0.0 || u + v > 1.0)
                return false;

            // calculate t, ray intersects triangle
            t = Vector3.Dot(edge2, qvec) * inv_det;

            return true;
        }
        #endregion

        #region CreateRuntimeCollisionData
        /*internal override void CreateRuntimeCollisionData(CollisionType collType)
        {
            // Note: Also update AssetImporterMeshStaticMesh

            // Create 2D version of the mesh (individual triangles)
            Vector2[] allVertices2D = new Vector2[objectIndices.Length];
            for (int i = 0; i < objectIndices.Length; i++)
            {
                allVertices2D[i] = new Vector2(
                    objectVertices[objectIndices[i]].pos.X,
                    objectVertices[objectIndices[i]].pos.Y);
            }
            // Create 3D version of the mesh
            Vector3[] allVertices3D = new Vector3[objectVertices.Length];
            for (int i = 0; i < allVertices3D.Length; i++)
            {
                allVertices3D[i] = new Vector3(
                    objectVertices[i].pos.X, objectVertices[i].pos.Y,
                    objectVertices[i].pos.Z);
            }

            switch (collType)
            {
                case CollisionType.BoundingBox:
                    collisionData = CollisionDataBox.FromMesh(allVertices2D, allVertices3D);
                    break;

                case CollisionType.BoundingEllipse:
                    collisionData = CollisionDataEllipse.FromMesh(allVertices2D, allVertices3D);
                    break;

                case CollisionType.BoundingSphere:
                    collisionData = CollisionDataSphere.FromMesh(allVertices2D, allVertices3D);
                    break;

                case CollisionType.ConvexHull:
                    collisionData = CollisionDataConvexHull.FromMesh(allVertices2D, allVertices3D);
                    break;

                case CollisionType.SkeletonBox:
                    collisionData = CollisionDataSkeletonBox.FromMesh(allVertices2D, allVertices3D);
                    break;

                case CollisionType.Vertices:
                    collisionData = CollisionDataVertices.FromMesh(allVertices2D, allVertices3D);
                    break;
            }
        }*/
        #endregion

        #region CreateBoundingSphere
        internal override void CreateBoundingSphere()
        {
            // Create 3D version of the mesh
            Vector3[] allVertices3D = new Vector3[objectVertices.Length];
            for (int i = 0; i < allVertices3D.Length; i++)
            {
                allVertices3D[i] = new Vector3(
                    objectVertices[i].Position.X, objectVertices[i].Position.Y,
                    objectVertices[i].Position.Z);
            }

            BoundingSphere = BoundingSphere.CreateFromPoints(allVertices3D);
        }
        #endregion

        #region SetVertexData
        /// <summary>
        /// Sets the vertex and index data on the device
        /// </summary>
        public virtual void SetVertexData()
        {
            //AlkaronCoreGame.Core.GraphicsDevice.Indices = indexBuffer;
            //AlkaronCoreGame.Core.GraphicsDevice.SetVertexBuffer(vertexBuffer);
        }
        #endregion

        #region Render
        /// <summary>
        /// Renders the mesh transformed by the world matrix
        /// </summary>
        public virtual void Render()
        {
            SetVertexData();

            //AlkaronCoreGame.Core.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
            //    0, 0, NumberOfFaces);
        }
        #endregion

        #region IsValid
        /// <summary>
        /// Is static mesh valid?
        /// </summary>
        public override bool IsValid
        {
            get
            {
                return vertexBuffer != null && indexBuffer != null;
            }
        }
        #endregion
    }
}
