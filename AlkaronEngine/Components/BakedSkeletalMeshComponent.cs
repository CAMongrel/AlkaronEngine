using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
    public class BakedSkeletalMeshComponent : BaseComponent
    {
        public BakedSkeletalMesh BakedSkeletalMesh { get; set; }

        public BakedSkeletalMeshComponent(Vector3 setCenter)
           : base(setCenter)
        {
            CanBeRendered = true;
        }

        public override List<BaseRenderProxy> CreateRenderProxies()
        {
            List<BaseRenderProxy> resultList = base.CreateRenderProxies();

            Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                   Matrix.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                   Matrix.CreateTranslation(Center);

            BakedSkeletalMeshRenderProxy proxy = new BakedSkeletalMeshRenderProxy(BakedSkeletalMesh);
            proxy.WorldMatrix = worldMatrix;
            proxy.Material = BakedSkeletalMesh.Material;
            proxy.BoundingBox = new BoundingBox(Center + BakedSkeletalMesh.BoundingBox.Min, Center + BakedSkeletalMesh.BoundingBox.Max);

            resultList.Add(proxy);

            return resultList;
        }
    }
}
