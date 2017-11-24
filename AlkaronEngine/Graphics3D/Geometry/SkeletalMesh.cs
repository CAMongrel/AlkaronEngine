using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.Geometry
{
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

    public class SkeletalMesh
    {
        public Material Material { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public SkeletalMeshBone RootBone { get; set; }

        public SkeletalMesh()
        {
            
        }
    }
}
