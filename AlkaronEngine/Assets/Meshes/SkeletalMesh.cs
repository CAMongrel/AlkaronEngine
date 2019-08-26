using AlkaronEngine.Assets.Importers;
using AlkaronEngine.Graphics;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace AlkaronEngine.Assets.Meshes
{
    /// <summary>
    /// Collada model. Supports bones and animation for collada (.dae) exported
    /// 3D Models from 3D Studio Max (8 or 9).
    /// This class is just for testing and it will only display one single mesh
    /// with bone and skinning support, the mesh also can only have one single
    /// material. Bones can be either in matrix mode or stored with transform
    /// and rotation values. SkinnedTangentVertex is used to store the vertices.
    /// </summary>
    public class SkeletalMesh : MeshAsset
    {
        protected override int MaxAssetVersion => 1;

        #region RuntimeBone helper class
        /// <summary>
        /// Bone
        /// </summary>
        public class RuntimeBone
        {
            #region Variables
            /// <summary>
            /// Parent bone, very important to get all parent matrices when
            /// building the finalMatrix for rendering.
            /// </summary>
            public RuntimeBone? parent = null;

            /// <summary>
            /// Children bones, not really used anywhere except for the ShowBones
            /// helper method, but also useful for debugging.
            /// </summary>
            public RuntimeBone[] children;

            /// <summary>
            /// Position, very useful to position bones to show bones in 3D, also
            /// only used for debugging and testing purposes.
            /// </summary>
            public Vector3 pos;

            /// <summary>
            /// Initial matrix we get from loading the collada model, it contains
            /// the start position and is used for the calculation to get the
            /// absolute and final matrices (see below).
            /// </summary>
            public Matrix4x4 initialMatrix;

            /// <summary>
            /// Id and name of this bone, makes debugging and testing easier, but
            /// it is also used to identify this bone later on in the loading process
            /// because our bone order might be different from the one in the file.
            /// </summary>
            public string id;

            /// <summary>
            /// invBoneMatrix is a special helper matrix loaded directly from
            /// the collada file. It is used to transform the final matrix
            /// back to a relative format after transforming and rotating each
            /// bone with the current animation frame. This is very important
            /// because else we would always move and rotate vertices around the
            /// center, but thanks to this inverted skin matrix the correct
            /// rotation points are used.
            /// </summary>
            public Matrix4x4 invBoneSkinMatrix;

            /// <summary>
            /// Final absolute matrix, which is calculated in UpdateAnimation each
            /// frame after all the loading is done. It can directly be used to
            /// find out the current bone positions, but for rendering we have
            /// to apply the invBoneSkinMatrix first to transform all vertices into
            /// the local space.
            /// </summary>
            private Matrix4x4 finalMatrix;

            public Matrix4x4 FinalMatrix
            {
                get
                {
                    if (parent == null)
                    {
                        return finalMatrix;
                    }

                    return finalMatrix * parent.FinalMatrix;
                }
                set
                {
                    finalMatrix = value;
                }
            }

            /// <summary>
            /// A custom tag for this bone.
            /// </summary>
            public int Tag;
            #endregion

            #region Constructor
            internal RuntimeBone()
            {
            }

            /// <summary>
            /// Create bone
            /// </summary>
            /// <param name="setMatrix">Set matrix</param>
            /// <param name="setParentBone">Set parent bone</param>
            /// <param name="setNum">Set number</param>
            /// <param name="setId">Set id name</param>
            public RuntimeBone(Matrix4x4 setMatrix, RuntimeBone setParentBone,
                int setNum, string setId)
            {
                initialMatrix = setMatrix;
                pos = initialMatrix.Translation;
                parent = setParentBone;
                id = setId;

                invBoneSkinMatrix = Matrix4x4.Identity;
            } // Bone(setMatrix, setParentBone, setNum)
            #endregion

            #region Get matrix recursively helper method
            /// <summary>
            /// Get matrix recursively
            /// </summary>
            /// <returns>Matrix</returns>
            public Matrix4x4 GetMatrixRecursively()
            {
                Matrix4x4 ret = initialMatrix;

                // If we have a parent mesh, we have to multiply the matrix with the
                // parent matrix.
                if (parent != null)
                {
                    ret *= parent.GetMatrixRecursively();
                }

                return ret;
            } // GetMatrixRecursively()
            #endregion

            #region To string
            /// <summary>
            /// To string, useful for debugging and testing.
            /// </summary>
            /// <returns>String</returns>
            public override string ToString()
            {
                return "Bone: Id=" + id + ", Position=" + pos;
            } // ToString()
            #endregion
        } // class Bone
        #endregion

        #region RuntimeSocket helper class
        /// <summary>
        /// Runtime socket
        /// </summary>
        public class RuntimeSocket
        {
            #region Variables
            /// <summary>
            /// Name
            /// </summary>
            /// <returns>String</returns>
            public string Name { get; set; } // Name

            /// <summary>
            /// Owner bone
            /// </summary>
            public RuntimeBone OwnerBone { get; private set; } // OwnerBone

            /// <summary>
            /// Translation
            /// </summary>
            /// <returns>Vector 3</returns>
            public Vector3 Translation { get; set; } // Translation

            /// <summary>
            /// Rotation
            /// </summary>
            /// <returns>Vector 3</returns>
            public Vector3 Rotation { get; set; } // Rotation

            /// <summary>
            /// Scaling
            /// </summary>
            /// <returns>Vector 3</returns>
            public Vector3 Scaling { get; set; } // Scaling

            /// <summary>
            /// Transform matrix
            /// </summary>
            /// <returns>Matrix</returns>
            public Matrix4x4 TransformMatrix
            {
                get
                {
                    // TODO: Cache this
                    return Matrix4x4.CreateScale(Scaling) *
                        Matrix4x4.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                        Matrix4x4.CreateTranslation(Translation);
                } // get
            } // TransformMatrix
            #endregion

            #region RuntimeSocket
            /// <summary>
            /// Create runtime socket
            /// </summary>
            /// <param name="ownerBone">Owner bone</param>
            public RuntimeSocket()
            {
                Name = "";
                OwnerBone = null;
            } // RuntimeSocket(ownerBone)
            #endregion

            #region SaveToStream
            /// <summary>
            /// Save to stream
            /// </summary>
            /// <param name="writer">Writer</param>
            internal void SaveToStream(BinaryWriter writer)
            {
                writer.Write(Name);

                if (OwnerBone == null)
                    writer.Write("");
                else
                    writer.Write(OwnerBone.id);

                writer.Write(Translation.X);
                writer.Write(Translation.Y);
                writer.Write(Translation.Z);

                writer.Write(Rotation.X);
                writer.Write(Rotation.Y);
                writer.Write(Rotation.Z);

                writer.Write(Scaling.X);
                writer.Write(Scaling.Y);
                writer.Write(Scaling.Z);
            } // SaveToStream(writer)
            #endregion

            #region LoadFromStream
            /// <summary>
            /// Load from stream
            /// </summary>
            /// <param name="reader">Reader</param>
            internal void LoadFromStream(BinaryReader reader, RuntimeBone[] boneList)
            {
                Name = reader.ReadString();

                string boneName = reader.ReadString();
                OwnerBone = null;
                for (int i = 0; i < boneList.Length; i++)
                {
                    if (boneList[i].id == boneName)
                    {
                        OwnerBone = boneList[i];
                        break;
                    } // if (boneList[i].id)
                }

                Translation = new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle());
                Rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle());
                Scaling = new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle());
            } // LoadFromStream(reader)
            #endregion
        } // class RuntimeSocket
        #endregion

        #region Variables
        /// <summary>
        /// Vertices for the main mesh (we only support one mesh here!).
        /// </summary>
        private SkinnedTangentVertex[] objectVertices;

        /// <summary>
        /// Number of vertices and number of indices we got in the
        /// vertex and index buffers.
        /// </summary>
        private int numOfVertices = 0;
        private int numOfIndices = 0;

        /// <summary>
        /// Object matrix for our mesh. Often used to fix mesh to bone skeleton.
        /// </summary>
        private Matrix4x4 objectMatrix = Matrix4x4.Identity;

        /// <summary>
        /// Animations
        /// </summary>
        internal List<BoneAnimationAsset> Animations { get; private set; } = new List<BoneAnimationAsset>();

        private BoneAnimationAsset? activeAnimation = null;

        /// <summary>
        /// Flat list of bones, the first bone is always the root bone, all
        /// children can be accessed from here. The main reason for having a flat
        /// list is easy access to all bones for showing bone previous and of
        /// course to quickly access all animation matrices.
        /// </summary>
        private RuntimeBone[] bones;

        /// <summary>
        /// List of all sockets
        /// </summary>
        internal RuntimeSocket[] sockets;
        #endregion

        #region Properties
        /// <summary>
        /// Was the model loaded successfully?
        /// </summary>
        public bool Loaded
        {
            get
            {
                return vertexBuffer != null &&
                    indexBuffer != null;
            } // get
        } // Loaded

        /// <summary>
        /// Total number of bones
        /// </summary>
        /// <returns>Int</returns>
        public int TotalNumberOfBones
        {
            get
            {
                return bones.Length;
            } // get
        } // TotalNumberOfBones

        /// <summary>
        /// Number of vertices of this mesh
        /// </summary>
        public int NumberOfVertices
        {
            get
            {
                return numOfVertices;
            }
        }

        /// <summary>
        /// Number of faces of this mesh
        /// </summary>
        public int NumberOfFaces
        {
            get
            {
                return numOfIndices / 3;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a model from a collada file
        /// </summary>
        /// <param name="setName">Set name</param>
        public SkeletalMesh()
        {
            sockets = new RuntimeSocket[0];
        } // ColladaModel(setFilename)
        #endregion

        #region FromVertices
        /// <summary>
        /// Creates a static mesh directly from vertices (triangles only)
        /// </summary>
        public static SkeletalMesh FromVertices(Vector3[] vertices, GraphicsDevice graphicsDevice, bool calcTangents)
        {
            SkinnedTangentVertex[] verts = new SkinnedTangentVertex[vertices.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = new SkinnedTangentVertex(
                    vertices[i], Vector2.Zero, new Vector3(0, 1, 0), Vector3.Zero, 
                    new Vector4(0, 0, 0, 0), Vector4.One);
            }

            return FromVertices(verts, graphicsDevice, calcTangents);
        }

        /// <summary>
        /// Creates a static mesh directly from tangent vertices (triangles only)
        /// </summary>
        public static SkeletalMesh FromVertices(SkinnedTangentVertex[] vertices, GraphicsDevice graphicsDevice, bool calcTangents)
        {
            uint[] indices = new uint[vertices.Length];
            for (uint i = 0; i < vertices.Length; i++)
            {
                indices[i] = i;
            }

            return FromVertices(vertices, indices, null, graphicsDevice, calcTangents);
        }

        /// <summary>
        /// Creates a static mesh directly from vertices and indices
        /// </summary>
        public static SkeletalMesh FromVertices(SkinnedTangentVertex[] vertices,
            uint[] indices, RuntimeBone[]? bones, GraphicsDevice graphicsDevice, bool calcTangents)
        {
            SkeletalMesh mesh = new SkeletalMesh();

            mesh.objectVertices = vertices;
            mesh.objectIndices = indices;

            if (calcTangents)
            {
                mesh.CalculateTangents();
            }

            mesh.bones = bones;

            BufferDescription vertexBufferDesc = new BufferDescription((uint)(SkinnedTangentVertex.SizeInBytes * mesh.objectVertices.Length), BufferUsage.VertexBuffer);
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

        internal override void CalculateTangents()
        {
            base.CalculateTangents();

            Vector3[] tangents = new Vector3[objectVertices.Length];
            Vector3[] tangents2 = new Vector3[objectVertices.Length];

            for (int i = 0; i < objectIndices.Length; i += 3)
            {
                uint i1 = objectIndices[i + 0];
                uint i2 = objectIndices[i + 1];
                uint i3 = objectIndices[i + 2];

                SkinnedTangentVertex vert1 = objectVertices[i1];
                SkinnedTangentVertex vert2 = objectVertices[i2];
                SkinnedTangentVertex vert3 = objectVertices[i3];

                float x1 = vert2.Position.X - vert1.Position.X;
                float x2 = vert3.Position.X - vert1.Position.X;
                float y1 = vert2.Position.Y - vert1.Position.Y;
                float y2 = vert3.Position.Y - vert1.Position.Y;
                float z1 = vert2.Position.Z - vert1.Position.Z;
                float z2 = vert3.Position.Z - vert1.Position.Z;

                float s1 = vert2.TexCoord.X - vert1.TexCoord.X;
                float s2 = vert3.TexCoord.X - vert1.TexCoord.X;
                float t1 = vert2.TexCoord.Y - vert1.TexCoord.Y;
                float t2 = vert3.TexCoord.Y - vert1.TexCoord.Y;

                float r = 1.0F / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tangents[i1] += sdir;
                tangents[i2] += sdir;
                tangents[i3] += sdir;

                tangents2[i1] += tdir;
                tangents2[i2] += tdir;
                tangents2[i3] += tdir;
            }

            for (int i = 0; i < objectVertices.Length; i++)
            {
                Vector3 n = objectVertices[i].Normal;
                Vector3 t = tangents[i];

                // Gram-Schmidt orthogonalize                
                objectVertices[i].Tangent = Vector3.Normalize(t - n * Vector3.Dot(n, t));

                // Calculate handedness
                float w = (Vector3.Dot(Vector3.Cross(n, t), tangents2[i]) < 0.0F) ? -1.0f : 1.0f;
            }
        }

        #region Load
        /// <summary>
        /// Load
        /// </summary>
        internal override void Deserialize(BinaryReader reader, AssetSettings assetSettings)
        {
            base.Deserialize(reader, assetSettings);

            // Read mesh data
            numOfVertices = reader.ReadInt32();
            objectVertices = new SkinnedTangentVertex[numOfVertices];
            for (int i = 0; i < objectVertices.Length; i++)
            {
                objectVertices[i] = new SkinnedTangentVertex(
                    new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle()),
                    new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle()),
                    new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle()),
                    new Vector4(reader.ReadInt32(), reader.ReadInt32(),
                        reader.ReadInt32(), reader.ReadInt32()),
                    new Vector4(reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle()));
            } // for (int)

            numOfIndices = reader.ReadInt32();
            objectIndices = new uint[numOfIndices];
            for (int i = 0; i < objectIndices.Length; i++)
            {
                objectIndices[i] = reader.ReadUInt32();
            } // for (int)

            BoundingSphere = new BoundingSphere(
                new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle()),
                reader.ReadSingle());

            // Read animation data
            // TODO

            string semantic = reader.ReadString();
            string defaultDiffuseTexture = reader.ReadString();
            semantic = reader.ReadString();
            string defaultNormalTexture = reader.ReadString();

            objectMatrix = ReadMatrixHelper(reader);

            int boneCnt = reader.ReadInt32();
            bones = new RuntimeBone[boneCnt];
            // Precreate empty bones
            for (int i = 0; i < boneCnt; i++)
            {
                bones[i] = new RuntimeBone();
            } // for (int)
              // Now read bones
            for (int i = 0; i < boneCnt; i++)
            {
                bones[i].id = reader.ReadString();
                bones[i].parent = ReadBoneRef(reader);
                int childCnt = reader.ReadInt32();
                bones[i].children = new RuntimeBone[childCnt];
                for (int j = 0; j < childCnt; j++)
                {
                    bones[i].children[j] = ReadBoneRef(reader);
                } // for (int)
                bones[i].initialMatrix = ReadMatrixHelper(reader);
                bones[i].invBoneSkinMatrix = ReadMatrixHelper(reader);
                int aninMatCnt = reader.ReadInt32();
                /*bones[i].animationMatrices = new Matrix4x4[aninMatCnt];
                for (int j = 0; j < aninMatCnt; j++)
                {
                    bones[i].animationMatrices[j] =
                        ReadMatrixHelper(reader);
                } // for (int)*/
            } // for (int)

            /*
            for (int i = 0; i < bones.Length; i++)
            {
                RuntimeBone bone = bones[i];

                // Just assign the final matrix from the animation matrices.
                bone.finalMatrix = bone.animationMatrices[0];

                // Also use parent matrix if we got one
                // This will always work because all the bones are in order.
                if (bone.parent != null)
                    bone.finalMatrix *=
                        bone.parent.finalMatrix;
            } // for (int)
            */

            sockets = new RuntimeSocket[reader.ReadInt32()];
            for (int i = 0; i < sockets.Length; i++)
            {
                sockets[i] = new RuntimeSocket();
                sockets[i].LoadFromStream(reader, bones);
            } // for (int)

            GenerateVertexAndIndexBuffers(assetSettings.GraphicsDevice);

            CreateBoundingSphere();
        } // Load(packageName, assetName, stream)
        #endregion

        #region Save
        /// <summary>
        /// Save
        /// </summary>
        /// <param name="writer">Writer</param>
        internal override void Serialize(BinaryWriter writer, AssetSettings assetSettings)
		{
            base.Serialize(writer, assetSettings);

            /*
            #region Write vertex data
			writer.Write(vertices.Length);
			for (int i = 0; i < vertices.Length; i++)
			{
				writer.Write(vertices[i].pos.X);
				writer.Write(vertices[i].pos.Y);
				writer.Write(vertices[i].pos.Z);

				writer.Write(vertices[i].uv.X);
				writer.Write(vertices[i].uv.Y);

				writer.Write(vertices[i].normal.X);
				writer.Write(vertices[i].normal.Y);
				writer.Write(vertices[i].normal.Z);

				writer.Write(vertices[i].tangent.X);
				writer.Write(vertices[i].tangent.Y);
				writer.Write(vertices[i].tangent.Z);

				writer.Write(vertices[i].bitangent.X);
				writer.Write(vertices[i].bitangent.Y);
				writer.Write(vertices[i].bitangent.Z);

				writer.Write(vertices[i].jointIndices.X);
				writer.Write(vertices[i].jointIndices.Y);
				writer.Write(vertices[i].jointIndices.Z);
				writer.Write(vertices[i].jointIndices.W);

				writer.Write(vertices[i].jointWeights.X);
				writer.Write(vertices[i].jointWeights.Y);
				writer.Write(vertices[i].jointWeights.Z);
				writer.Write(vertices[i].jointWeights.W);
			} // for (int)
            #endregion

            #region Write indices
			writer.Write(objectIndices.Length);
			for (int i = 0; i < objectIndices.Length; i++)
			{
				writer.Write(objectIndices[i]);
			} // for (int)
            #endregion

            #region Write collision data
			// Write collision info
			// New in version 4
			WriteCollisionData(writer, collisionData);

			// New in version 8
			writer.Write((int)customCollisions.Count);
			for (int i = 0; i < customCollisions.Count; i++)
			{
				WriteCollisionData(writer, customCollisions[i]);
			} // for (int)
            #endregion

            #region Write animation data
			// New in version 5
			writer.Write(runtimeAnimations.Count);
			for (int i = 0; i < runtimeAnimations.Count; i++)
			{
				writer.Write(runtimeAnimations[i].Name);
				writer.Write(runtimeAnimations[i].Start);
				writer.Write(runtimeAnimations[i].End);
			} // for (int)
            #endregion

            #region Write object matrix
			HellspawnEngine.Assets.Importers.AssetImporterMesh.
				WriteMatrixHelper(writer, objectMatrix);
            #endregion

			writer.Write(numOfAnimations);

            #region Write bone list
			writer.Write(bones.Length);
			for (int i = 0; i < bones.Length; i++)
			{
				writer.Write(bones[i].id);
				WriteBoneRef(bones[i].parent, writer);
				writer.Write(bones[i].children.Length);
				for (int j = 0; j < bones[i].children.Length; j++)
				{
					WriteBoneRef(bones[i].children[j], writer);
				} // for (int)
				HellspawnEngine.Assets.Importers.AssetImporterMesh.
					WriteMatrixHelper(writer, bones[i].initialMatrix);
				HellspawnEngine.Assets.Importers.AssetImporterMesh.
					WriteMatrixHelper(writer, bones[i].invBoneSkinMatrix);
				writer.Write(bones[i].animationMatrices.Length);
				for (int j = 0; j < bones[i].animationMatrices.Length; j++)
				{
					HellspawnEngine.Assets.Importers.AssetImporterMesh.
						WriteMatrixHelper(writer, bones[i].animationMatrices[j]);
				} // for (int)
			} // for (int)
            #endregion

			// Write node material name
			writer.Write(nodeMaterial != null ? nodeMaterial.Fullname : "");

            #region Write sockets
			writer.Write(sockets.Length);
			for (int i = 0; i < sockets.Length; i++)
			{
				sockets[i].SaveToStream(writer);
			} // for (int)
            #endregion
            */
		} // Save(writer)
        #endregion
		
        #region WriteBoneRef
		void WriteBoneRef(RuntimeBone bone, BinaryWriter writer)
		{
			for (int i = 0; i < bones.Length; i++)
			{
				if (bones[i] == bone)
				{
					writer.Write(i);
					return;
				}
			}
			
			writer.Write((int)-1);
		}
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

        #region ReadMatrixHelper
        private static Matrix4x4 ReadMatrixHelper(BinaryReader reader)
        {
            return new Matrix4x4(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle());
        }
        #endregion

        #region ReadBoneRef
        private RuntimeBone ReadBoneRef(BinaryReader reader)
        {
            int boneIdx = reader.ReadInt32();
            if (boneIdx < 0 ||
                boneIdx >= bones.Length)
                return null;

            return bones[boneIdx];
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose stuff that need to be disposed in XNA.
        /// </summary>
        public override void Dispose()
        {
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
        } // Dispose()
        #endregion

        #region IsValid
        /// <summary>
        /// Is skeletal mesh valid?
        /// </summary>
        public override bool IsValid
        {
            get
            {
                return vertexBuffer != null && indexBuffer != null;
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

        #region Generate vertex and index buffers
        /// <summary>
        /// Generate vertex and index buffers
        /// </summary>
        private void GenerateVertexAndIndexBuffers(GraphicsDevice graphicsDevice)
        {
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

            BufferDescription vertexBufferDesc = new BufferDescription((uint)(SkinnedTangentVertex.SizeInBytes * objectVertices.Length), BufferUsage.VertexBuffer);
            vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(vertexBufferDesc);

            BufferDescription indexBufferDesc = new BufferDescription((uint)(sizeof(int) * objectIndices.Length), BufferUsage.IndexBuffer);
            indexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(indexBufferDesc);

            graphicsDevice.UpdateBuffer(vertexBuffer, 0, objectVertices);
            graphicsDevice.UpdateBuffer(indexBuffer, 0, objectIndices);

            numOfIndices = objectIndices.Length;
        } // GenerateVertexAndIndexBuffers()
        #endregion

        #region Update animation
        public string? ActiveAnimationIdentifier
        {
            get
            {                
                return activeAnimation?.AnimationIdentifier ?? null;
            }
            set
            {
                SetActiveAnimation(value);
            }
        }

        private void SetActiveAnimation(string? identifier)
        {
            if (identifier == null)
            {
                activeAnimation = null;
                return;
            }

            identifier = identifier.ToLowerInvariant();

            if (activeAnimation != null)
            {
                if (activeAnimation.AnimationIdentifier.ToLowerInvariant() == identifier)
                {
                    // Don't set the same animation
                    return;
                }
            }

            activeAnimation = (from a in Animations
                               where a.AnimationIdentifier.ToLowerInvariant() == identifier
                               select a).FirstOrDefault();
        }

        /// <summary>
        /// Updates bone locations with the animation time.
        /// 
        /// Should be called from the RenderThread from a RenderProxy
        /// </summary>
        public void SetAnimationTime(double animTime)
        {
            var actAnim = activeAnimation;
            if (actAnim == null)
            {
                return;
            }

            for (int i = 0; i < bones.Length; i++)
            {
                RuntimeBone bone = bones[i];
                AnimationBoneData data = actAnim.DataByBoneIndex(bone.Tag);

                // Just assign the final matrix from the animation matrices.
                bone.FinalMatrix = data?.GetMatrix(animTime) ?? Matrix4x4.Identity;
            } // foreach
        } // UpdateAnimation()
        #endregion

        #region GetBoneMatrices
        /// <summary>
        /// Get bone matrices for the shader. We have to apply the invBoneSkinMatrix
        /// to each final matrix, which is the recursively created matrix from
        /// all the animation data (see UpdateAnimation).
        /// </summary>
        /// <returns></returns>
        public Matrix4x4[] GetBoneMatrices(Matrix4x4 renderMatrix)
        {
            //Matrix4x4.Invert(renderMatrix, out var renderMatrixInverse);
            Matrix4x4.Invert(bones[0].initialMatrix, out var renderMatrixInverse);

            // And get all bone matrices, we support max. 80 (see shader).
            Matrix4x4[] matrices = new Matrix4x4[Math.Min(80/*SkinnedEffect.MaxBones*/, bones.Length)];
            for (int num = 0; num < matrices.Length; num++)
            {
                // The matrices are constructed from the invBoneSkinMatrix and
                // the finalMatrix, which holds the recursively added animation matrices
                // and finally we add the render matrix too here.
                matrices[num] =
                    bones[num].invBoneSkinMatrix *
                    bones[num].FinalMatrix *
                    renderMatrixInverse
                    ;
            }

            return matrices;
        } // GetBoneMatrices()
        #endregion

        #region RenderSkeleton
        /// <summary>
        /// Renders the skeleton of this skeletal mesh
        /// </summary>
        public void RenderSkeleton(Matrix4x4 worldMatrix)
        {
            /*CompareFunction oldFunc =
                AlkaronCoreGame.Core.GraphicsDevice.DepthStencilState.DepthBufferFunction;
            AlkaronCoreGame.Core.GraphicsDevice.DepthStencilState.DepthBufferFunction =
                CompareFunction.Always;

            RenderBone(worldMatrix, bones[0]);

            AlkaronCoreGame.Core.GraphicsDevice.DepthStencilState.DepthBufferFunction =
                oldFunc;*/
        }
        #endregion

        #region RenderBone
        /// <summary>
        /// Renders a single bone and all its childs using lines.
        /// </summary>
        private void RenderBone(Matrix4x4 worldMatrix, RuntimeBone bone)
        {
            for (int childIdx = 0; childIdx < bone.children.Length; childIdx++)
            {
                // Draw line to child
                // TODO!!!
                /*PrimitiveRenderer.DrawLine(
					worldMatrix,
					bone.finalMatrix.Translation,
					bone.children[childIdx].finalMatrix.Translation,
					new Vector4(1, 0, 0, 1));*/

                // Render childs of child
                RenderBone(worldMatrix, bone.children[childIdx]);
            }
        }
        #endregion

        #region GetBoneLocation
        /// <summary>
        /// Get bone location in local space
        /// </summary>
        public Vector3 GetBoneLocation(string BoneName)
        {
            BoneName = BoneName.ToLowerInvariant();

            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i].id.ToLowerInvariant() == BoneName)
                {
                    return bones[i].FinalMatrix.Translation;
                } // if (bones[i].id.ToLowerInvariant)
            } // for (int)

            return Vector3.Zero;
        } // GetBoneWorldLocation(BoneName)
        #endregion

        #region GetBoneNames
        /// <summary>
        /// Get bone names
        /// </summary>
        /// <returns>List</returns>
        public List<string> GetBoneNames()
        {
            List<string> resultList = new List<string>();
            for (int i = 0; i < bones.Length; i++)
            {
                resultList.Add(bones[i].id);
            }
            return resultList;
        }
        #endregion // GetBoneNames()

        #region GetSocketLocation
        /// <summary>
        /// Get socket location
        /// </summary>
        public Vector3 GetSocketLocation(string SocketName)
        {
            RuntimeSocket socket = null;
            for (int i = 0; i < sockets.Length; i++)
            {
                if (sockets[i].Name == SocketName)
                {
                    socket = sockets[i];
                    break;
                } // if (sockets[i].Name)
            }

            if (socket == null ||
                socket.OwnerBone == null)
            {
                return Vector3.Zero;
            }

            return socket.OwnerBone.FinalMatrix.Translation + socket.Translation;
        } // GetSocketLocation(SocketName)
        #endregion

        #region SetVertexData
        /// <summary>
        /// Sets the vertex and index data on the device
        /// </summary>
        public virtual void SetVertexData(RenderContext renderContext)
        {
            //Matrix4x4[] boneMats = GetBoneMatrices(Matrix4x4.Identity);

            Performance.StartAppendAggreate("SetVertexBuffer");
            renderContext.CommandList.SetVertexBuffer(0, vertexBuffer);
            renderContext.CommandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);
            Performance.EndAppendAggreate("SetVertexBuffer");
        }
        #endregion

        #region Render
        /// <summary>
        /// Renders the mesh transformed by the world matrix
        /// </summary>
        public virtual void Render(RenderContext renderContext)
        {
            SetVertexData(renderContext);

            Performance.StartAppendAggreate("DrawPrimitives");
            renderContext.CommandList.DrawIndexed((uint)objectIndices.Length);
            Performance.EndAppendAggreate("DrawPrimitives");
        }
        #endregion
    }
} // namespace SkinningWithColladaModelsInXna.Graphics
