using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Graphics3D.RenderProxies;
using System.Collections.Generic;
using System.Numerics;

namespace AlkaronEngine.Components
{
    public class StaticMeshComponent : BaseComponent
    {
        protected List<StaticMesh> staticMeshes;

        public StaticMeshComponent(Vector3 setCenter)
           : base(setCenter)
        {
            CanBeRendered = true;
            staticMeshes = new List<StaticMesh>();
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (IsDirty)
            {
                CreateRenderProxies();

                IsDirty = false;
            }
        }

        private void CreateRenderProxies()
        {
            List<BaseRenderProxy> resultList = new List<BaseRenderProxy>();

            Matrix4x4 worldMatrix = Matrix4x4.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                   Matrix4x4.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                   Matrix4x4.CreateTranslation(Center);

            for (int i = 0; i < staticMeshes.Count; i++)
            {
                StaticMesh mesh = staticMeshes[i];

                StaticMeshRenderProxy proxy = new StaticMeshRenderProxy(mesh);
                proxy.WorldMatrix = worldMatrix * mesh.RootTransform;
                proxy.Material = mesh.Material;
                //proxy.BoundingBox = BoundingBox.CreateMerged(proxy.BoundingBox, BoundingBox.CreateFromSphere(staticMeshes[i].BoundingSphere));
                resultList.Add(proxy);
            }

            renderProxies = resultList.ToArray();
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
