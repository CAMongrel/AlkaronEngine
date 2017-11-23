using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.Geometry
{
    public class SkeletalMesh
    {
        public Material Material { get; set; }

        public VertexBuffer VertexBuffer { get; set; }
        public PrimitiveType PrimitiveType { get; set; }
        public int PrimitiveCount { get; set; }

        public SkeletalMesh()
        {
        }
    }
}
