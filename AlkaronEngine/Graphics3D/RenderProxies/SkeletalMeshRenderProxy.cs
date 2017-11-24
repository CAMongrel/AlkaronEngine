using System;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Util;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
    public class SkeletalMeshRenderProxy : BaseRenderProxy
    {
        public SkeletalMesh SkeletalMesh { get; private set; }

        public SkeletalMeshRenderProxy(SkeletalMesh setSkeletalMesh)
        {
            SkeletalMesh = setSkeletalMesh;
        }

        private void RenderBone(SkeletalMeshBone bone, 
                                RenderManager renderManager,
                                Graphics2D.IRenderConfiguration renderConfig)
        {
            StaticMesh mesh = bone.Mesh;
            if (mesh != null)
            {
                mesh.Material.Effect.Parameters["WorldViewProj"].SetValue(bone.CombinedTransform * renderManager.ViewTarget.ViewMatrix * 
                                                                          renderManager.ViewTarget.ProjectionMatrix);
                mesh.Material.Effect.CurrentTechnique.Passes[0].Apply();

                if (mesh.DiffuseTexture != null)
                {
                    mesh.Material.Effect.Parameters["Texture"].SetValue(mesh.DiffuseTexture);
                    mesh.Material.Effect.CurrentTechnique.Passes[0].Apply();
                }

                renderConfig.GraphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
                renderConfig.GraphicsDevice.DrawPrimitives(mesh.PrimitiveType, 0, mesh.PrimitiveCount);
            }

            for (int i = 0; i < bone.ChildBones.Length; i++)
            {
                RenderBone(bone.ChildBones[i], renderManager, renderConfig);
            }
        }

        public override void Render(Graphics2D.IRenderConfiguration renderConfig, RenderManager renderManager)
        {
            base.Render(renderConfig, renderManager);

            RenderBone(SkeletalMesh.RootBone, renderManager, renderConfig);

            /*Performance.StartAppendAggreate("Setup");
            StaticMesh.Material.Effect.Parameters["WorldViewProj"].SetValue(WorldMatrix * renderManager.ViewTarget.ViewMatrix * renderManager.ViewTarget.ProjectionMatrix);
            StaticMesh.Material.Effect.CurrentTechnique.Passes[0].Apply();
            Performance.EndAppendAggreate("Setup");

            if (StaticMesh.DiffuseTexture != null)
            {
                Performance.StartAppendAggreate("Setup Texture");
                StaticMesh.Material.Effect.Parameters["Texture"].SetValue(StaticMesh.DiffuseTexture);
                StaticMesh.Material.Effect.CurrentTechnique.Passes[0].Apply();
                Performance.EndAppendAggreate("Setup Texture");
            }

            Performance.StartAppendAggreate("SetVertexBuffer");
            renderConfig.GraphicsDevice.SetVertexBuffer(StaticMesh.VertexBuffer);
            Performance.EndAppendAggreate("SetVertexBuffer");
            Performance.StartAppendAggreate("DrawPrimitives");
            renderConfig.GraphicsDevice.DrawPrimitives(StaticMesh.PrimitiveType, 0, StaticMesh.PrimitiveCount);
            Performance.EndAppendAggreate("DrawPrimitives");*/
        }
    }
}
