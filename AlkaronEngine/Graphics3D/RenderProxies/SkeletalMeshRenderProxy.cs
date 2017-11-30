using System;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
    public class SkeletalMeshRenderProxy : BaseRenderProxy
    {
        public SkeletalMesh SkeletalMesh { get; private set; }

        public SkeletalMeshRenderProxy(SkeletalMesh setSkeletalMesh)
        {
            SkeletalMesh = setSkeletalMesh;
        }

        private void RenderMeshPart(SkeletalMeshPart part, 
                                    RenderManager renderManager,
                                    Graphics2D.IRenderConfiguration renderConfig)
        {
            SkinnedEffect skinEff = part.Material.Effect as SkinnedEffect;
            if (skinEff == null)
            {
                return;
            }

            skinEff.WeightsPerVertex = 1;

            skinEff.AmbientLightColor = Vector3.One;
            skinEff.FogEnabled = false;

            skinEff.SetBoneTransforms(part.BoneMatrics);

            skinEff.World = WorldMatrix;
            skinEff.View = renderManager.ViewTarget.ViewMatrix;
            skinEff.Projection = renderManager.ViewTarget.ProjectionMatrix;

            skinEff.Parameters["WorldViewProj"].SetValue(WorldMatrix * 
                                                         renderManager.ViewTarget.ViewMatrix * 
                                                         renderManager.ViewTarget.ProjectionMatrix);
            part.Material.Effect.CurrentTechnique.Passes[0].Apply();

            if (part.DiffuseTexture != null)
            {
                part.Material.Effect.Parameters["Texture"].SetValue(part.DiffuseTexture);
                part.Material.Effect.CurrentTechnique.Passes[0].Apply();
            }

            renderConfig.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
            renderConfig.GraphicsDevice.DrawPrimitives(part.PrimitiveType, 0, part.PrimitiveCount);
        }

        public override void Render(Graphics2D.IRenderConfiguration renderConfig, RenderManager renderManager)
        {
            base.Render(renderConfig, renderManager);

            for (int i = 0; i < SkeletalMesh.MeshParts.Count; i++)
            {
                RenderMeshPart(SkeletalMesh.MeshParts[i], renderManager, renderConfig);
            }

            /*if (SkeletalMesh.Model != null)
            {
                SkeletalMesh.Model.Draw(WorldMatrix, renderManager.ViewTarget.ViewMatrix,
                                        renderManager.ViewTarget.ProjectionMatrix);                
            }
            else
            {
                RenderBone(SkeletalMesh.RootBone, renderManager, renderConfig);
            }*/

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
