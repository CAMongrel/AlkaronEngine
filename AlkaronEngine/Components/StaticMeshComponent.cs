using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Components
{
    public class StaticMeshComponent : BaseComponent
    {
        protected List<StaticMesh> staticMeshes;

        private BoundingBox meshesBoundingBox;

        public StaticMeshComponent(Vector3 setCenter)
           : base(setCenter)
        {
            CanBeRendered = true;
            staticMeshes = new List<StaticMesh>();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsDirty)
            {
                RebuildBoundingBox();

                IsDirty = false;
            }
        }

        /*public override BaseRenderProxy[] Draw(GameTime gameTime, RenderManager renderManager)
        {
            if (renderManager.CameraFrustum.Contains(BoundingBox) == ContainmentType.Disjoint)
            {
                return null;
            }

            if (rebuildRenderProxyCacheNextDrawCall)
            {
                Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                   Matrix.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                   Matrix.CreateTranslation(Center);

                List<BaseRenderProxy> result = new List<BaseRenderProxy>(staticMeshes.Count);
                for (int i = 0; i < staticMeshes.Count; i++)
                {
                    StaticMesh mesh = staticMeshes[i];

                    StaticMeshRenderProxy proxy = new StaticMeshRenderProxy(mesh);
                    proxy.WorldMatrix = worldMatrix;
                    proxy.Material = mesh.Material;
                    result.Add(proxy);
                }

                cachedProxies = result.ToArray();

                rebuildRenderProxyCacheNextDrawCall = false;
            }

            return cachedProxies;
        }*/

        public override List<BaseRenderProxy> CreateRenderProxies()
        {
            List<BaseRenderProxy> resultList = base.CreateRenderProxies();

            Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                   Matrix.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                   Matrix.CreateTranslation(Center);

            for (int i = 0; i < staticMeshes.Count; i++)
            {
                StaticMesh mesh = staticMeshes[i];

                StaticMeshRenderProxy proxy = new StaticMeshRenderProxy(mesh);
                proxy.WorldMatrix = worldMatrix;
                proxy.Material = mesh.Material;
                resultList.Add(proxy);
            }

            return resultList;
        }

        private void RebuildBoundingBox()
        {
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;

            meshesBoundingBox = new BoundingBox();
            for (int i = 0; i < staticMeshes.Count; i++)
            {
                meshesBoundingBox = BoundingBox.CreateMerged(BoundingBox, staticMeshes[i].BoundingBox);
            }

            BoundingBox = new BoundingBox(Center + meshesBoundingBox.Min, Center + meshesBoundingBox.Max);
        }

        public void ClearStaticMeshes()
        {
            staticMeshes.Clear();

            IsDirty = true;
        }

        public void AddStaticMesh(StaticMesh mesh)
        {
            staticMeshes.Add(mesh);

            IsDirty = true;
        }

        public void AddStaticMeshes(IEnumerable<StaticMesh> meshes)
        {
            staticMeshes.AddRange(meshes);

            IsDirty = true;
        }

        public void RemoveStaticMesh(StaticMesh mesh)
        {
            staticMeshes.Remove(mesh);

            IsDirty = true;
        }
    }
}
