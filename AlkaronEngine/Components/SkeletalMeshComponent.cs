using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
    public class SkeletalMeshComponent : BaseComponent
    {
        int frameIndex;
        double lastFrameTime;

        public SkeletalMesh SkeletalMesh { get; set; }

        // Reference to the rendering proxy
        private SkeletalMeshRenderProxy proxy;

        public SkeletalMeshComponent(Vector3 setCenter)
           : base(setCenter)
        {
            frameIndex = 0;
            CanBeRendered = true;
        }

        public override List<BaseRenderProxy> CreateRenderProxies()
        {
            List<BaseRenderProxy> resultList = base.CreateRenderProxies();

            Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                   Matrix.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                   Matrix.CreateTranslation(Center);

            if (SkeletalMesh != null)
            {
                proxy = new SkeletalMeshRenderProxy(SkeletalMesh);
                proxy.WorldMatrix = worldMatrix;
                proxy.Material = SkeletalMesh.Material;
                proxy.BoundingBox = new BoundingBox(Center + SkeletalMesh.BoundingBox.Min, Center + SkeletalMesh.BoundingBox.Max);

                resultList.Add(proxy);
            }

            return resultList;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (proxy != null)
            {
                proxy.TickAnimation(gameTime.ElapsedGameTime.TotalSeconds);
            }
        }
    }
}
