using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Graphics3D.RenderProxies;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Utilities;

namespace AlkaronEngine.Components
{
    public class SkeletalMeshComponent : BaseComponent
    {
        //private int frameIndex;
        //private double lastFrameTime;

        public SkeletalMesh SkeletalMesh { get; private set; }

        // Reference to the rendering proxy
        private SkeletalMeshRenderProxy proxy;

        public SkeletalMeshComponent(Vector3 setCenter)
           : base(setCenter)
        {
            //frameIndex = 0;
            CanBeRendered = true;
        }

        public void SetSkeletalMesh(SkeletalMesh skeletalMesh)
        {
            CreateRenderProxies();
        }

        private void CreateRenderProxies()
        {
            List<BaseRenderProxy> resultList = new List<BaseRenderProxy>();

            if (SkeletalMesh != null)
            {
                Matrix4x4 worldMatrix = Matrix4x4.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                       Matrix4x4.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                       Matrix4x4.CreateTranslation(Center);

                SkeletalMeshRenderProxy proxy = new SkeletalMeshRenderProxy(SkeletalMesh);
                proxy.WorldMatrix = worldMatrix;
                proxy.Material = SkeletalMesh.Material;
                //proxy.BoundingBox = BoundingBox.CreateFromSphere(new BoundingSphere(Center, SkeletalMesh.BoundingSphere.Radius));

                resultList.Add(proxy);
            }

            renderProxies = resultList.ToArray();
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            /*if (proxy != null)
            {
                proxy.TickAnimation(gameTime.ElapsedGameTime.TotalSeconds);
            }*/
        }
    }
}
