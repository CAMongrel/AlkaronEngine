using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.Geometry
{
    public struct Float4
    {
        private float f1;
        private float f2;
        private float f3;
        private float f4;

        public int Length
        {
            get { return 4; }
        }

        public float this[int index]
        {
            get 
            {
                switch (index)
                {
                    case 0: return f1;
                    case 1: return f2;
                    case 2: return f3;
                    case 3: return f4;
                    default: return f1;
                }
            }
            set
            {
                switch (index)
                {
                    case 0: f1 = value; break;
                    case 1: f2 = value; break;
                    case 2: f3 = value; break;
                    case 3: f4 = value; break;
                    default: f1 = value; break;
                }
            }
        }
    }

    public struct Byte4
    {
        private byte b1;
        private byte b2;
        private byte b3;
        private byte b4;

        public int Length
        {
            get { return 4; }
        }

        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return b1;
                    case 1: return b2;
                    case 2: return b3;
                    case 3: return b4;
                    default: return b1;
                }
            }
            set
            {
                switch (index)
                {
                    case 0: b1 = value; break;
                    case 1: b2 = value; break;
                    case 2: b3 = value; break;
                    case 3: b4 = value; break;
                    default: b1 = value; break;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SkeletalMeshVertex
    {
        public Vector3 Position;

        public Vector3 Normal;

        public Vector2 TextureCoordinate;

        public Byte4 BlendIndices; 

        public Float4 BlendWeights;

        public static VertexDeclaration VertexDeclaration => new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0),
            new VertexElement(36, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0)
        );

        public SkeletalMeshVertex(VertexPositionNormalTexture vpnt)
        {
            Position = vpnt.Position;
            Normal = vpnt.Normal;
            TextureCoordinate = vpnt.TextureCoordinate;

            BlendWeights = new Float4();
            BlendWeights[0] = 1.0f; BlendWeights[1] = 1.0f; BlendWeights[2] = 1.0f; BlendWeights[3] = 1.0f;
            BlendIndices = new Byte4();
        }

        private int FindBonePosition(int boneIdx)
        {
            for (int i = 0; i < BlendIndices.Length; i++)
            {
                if (BlendIndices[i] == boneIdx ||
                    BlendIndices[i] == 0)
                {
                    return i;
                }
            }

            throw new Exception("Can only have a maximum of four bones per vertex");
        }

        public void SetBoneData(float weight, byte boneIdx)
        {
            int pos = FindBonePosition(boneIdx);
            BlendIndices[pos] = boneIdx;
            BlendWeights[pos] = weight;
        }
    }

    public class SkeletalMeshBone
    {
        public int MeshIndex;
        public SkeletalMeshBone ParentBone;
        public SkeletalMeshBone[] ChildBones;
        public Matrix Transform;

        public Matrix CombinedTransform
        {
            get
            {
                if (ParentBone != null)
                {
                    return Transform * ParentBone.CombinedTransform;
                }

                return Transform;
            }
        }

        public StaticMesh Mesh;
    }

    public class SkeletalMeshPart
    {
        public VertexBuffer VertexBuffer { get; set; }
        public PrimitiveType PrimitiveType { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public int PrimitiveCount { get; set; }

        public Texture2D DiffuseTexture { get; set; }
        public Material Material { get; set; }

        public SkeletalMeshBone[] Bones { get; set; }
        public Matrix[] BoneMatrics { get; set; }
        public int[] AffectedBonesIndices { get; set; }
    }

    public class SkeletalMesh
    {
        public Material Material { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public SkeletalMeshBone[] Bones { get; set; }

        public List<SkeletalMeshPart> MeshParts { get; private set; }

        public SkeletalMesh()
        {
            MeshParts = new List<SkeletalMeshPart>();
        }
    }
}
