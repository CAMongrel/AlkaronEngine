using System;
using AlkaronEngine.Graphics3D.Geometry;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
    public class BakedSkeletalMeshRenderProxy : BaseRenderProxy
    {
        public BakedSkeletalMesh BakedSkeletalMesh { get; private set; }

        public BakedSkeletalMeshRenderProxy(BakedSkeletalMesh setSkeletalMesh)
        {
            BakedSkeletalMesh = setSkeletalMesh;
        }
    }
}
