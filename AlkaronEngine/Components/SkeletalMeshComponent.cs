using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
    public class SkeletalMeshComponent : BaseComponent
    {
        //private int frameIndex;
        //private double lastFrameTime;

        public SkeletalMesh SkeletalMesh { get; private set; }

        // Reference to the rendering proxy
        //private SkeletalMeshRenderProxy proxy;

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
                Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                       Matrix.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                       Matrix.CreateTranslation(Center);

                SkeletalMeshRenderProxy proxy = new SkeletalMeshRenderProxy(SkeletalMesh);
                proxy.WorldMatrix = worldMatrix;
                proxy.Material = SkeletalMesh.Material;
                proxy.BoundingBox = new BoundingBox(Center + SkeletalMesh.BoundingBox.Min, Center + SkeletalMesh.BoundingBox.Max);

                resultList.Add(proxy);
            }

            renderProxies = resultList.ToArray();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            /*if (proxy != null)
            {
                proxy.TickAnimation(gameTime.ElapsedGameTime.TotalSeconds);
            }*/
        }
    }
}
