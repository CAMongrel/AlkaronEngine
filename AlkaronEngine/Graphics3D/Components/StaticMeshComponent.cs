using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.Components
{
   public class StaticMeshComponent : BaseComponent
   {
      private List<StaticMesh> staticMeshes;

      private BaseRenderProxy[] cachedProxies;

      private BoundingBox meshesBoundingBox;

      public StaticMeshComponent(Vector3 setCenter)
         : base(setCenter)
      {
         cachedProxies = null;
         CanBeRendered = true;
         staticMeshes = new List<StaticMesh>();
      }

      public override BaseRenderProxy[] Draw(GameTime gameTime, RenderManager renderManager)
      {
         Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
            Matrix.CreateScale(Scale.X, Scale.Y, Scale.Z) *
            Matrix.CreateTranslation(Center);

         BaseRenderProxy[] result = new BaseRenderProxy[staticMeshes.Count];
         for (int i = 0; i < staticMeshes.Count; i++)
         {
            StaticMesh mesh = staticMeshes[i];

            StaticMeshRenderProxy proxy = new StaticMeshRenderProxy(mesh);
            proxy.WorldMatrix = worldMatrix;
            proxy.Material = mesh.Material;
            result[i] = proxy;
         }

         BoundingBox = new BoundingBox(Center + meshesBoundingBox.Min, Center + meshesBoundingBox.Max);

         cachedProxies = result;

         return result;
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
      }

      public void ClearStaticMeshes()
      {
         staticMeshes.Clear();

         meshesBoundingBox = new BoundingBox();
      }

      public void AddStaticMesh(StaticMesh mesh)
      {
         staticMeshes.Add(mesh);

         meshesBoundingBox = BoundingBox.CreateMerged(BoundingBox, mesh.BoundingBox);
      }

      public void AddStaticMeshes(IEnumerable<StaticMesh> meshes)
      {
         staticMeshes.AddRange(meshes);

         RebuildBoundingBox();
      }

      public void RemoveStaticMesh(StaticMesh mesh)
      {
         staticMeshes.Remove(mesh);

         RebuildBoundingBox();
      }
   }
}
