using System;
using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static AlkaronEngine.Assets.Meshes.SkeletalMesh;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
    public class SkeletalMeshRenderProxy : BaseRenderProxy
    {
        public static BasicEffect effect = null;

        public SkeletalMesh SkeletalMesh { get; private set; }

        /// <summary>
        /// Used by both threads, must be guarded by lock object
        /// </summary>
        private double nextAnimationTimeDelta;
        public double AnimationTime { get; private set; }

        public SkeletalMeshRenderProxy(SkeletalMesh setSkeletalMesh)
        {
            SkeletalMesh = setSkeletalMesh;
        }

        /*private void RenderMeshPart(SkeletalMeshPart part, 
                                    RenderManager renderManager,
                                    Graphics2D.IRenderConfiguration renderConfig, 
                                    Material materialToUse)
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
        }*/

        private void DrawBone(RuntimeBone bone, RenderManager renderManager, IRenderConfiguration renderConfig)
        {
            /*if (effect == null)
            {
                effect = new BasicEffect(renderConfig.GraphicsDevice);
            }

            VertexPositionColor v1 = new VertexPositionColor();
            v1.Color = Color.Red;
            v1.Position = bone.CombinedTransform.Translation;

            VertexPositionColor v2 = new VertexPositionColor();
            v2.Color = Color.Blue;
            v2.Position = bone.ParentBone.CombinedTransform.Translation;

            Vector3 midPoint = v1.Position + ((v2.Position - v1.Position) * 0.5f);
            Vector3 screenPos = renderConfig.GraphicsDevice.Viewport.Project(midPoint,
                                                                             renderManager.ViewTarget.ProjectionMatrix,
                                                                             renderManager.ViewTarget.ViewMatrix,
                                                                             WorldMatrix);
            renderConfig.PrimitiveRenderManager.SpriteBatch.Begin();
            renderConfig.PrimitiveRenderManager.SpriteBatch.DrawString(renderConfig.PrimitiveRenderManager.EngineFont,
                                                              bone.BoneName,
                                                              new Vector2(screenPos.X, screenPos.Y),
                                                              Color.Yellow);
            renderConfig.PrimitiveRenderManager.SpriteBatch.End();

            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
            effect.FogEnabled = false;
            effect.LightingEnabled = false;

            effect.Parameters["WorldViewProj"].SetValue(WorldMatrix *
                                                        renderManager.ViewTarget.ViewMatrix *
                                                        renderManager.ViewTarget.ProjectionMatrix);

            effect.CurrentTechnique.Passes[0].Apply();

            renderConfig.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList,
                                                                                new VertexPositionColor[] { v1, v2 }, 0, 1);

            for (int i = 0; i < bone.ChildBones.Length; i++)
            {
                DrawBone(bone.ChildBones[i], renderManager, renderConfig);
            }*/
        }

        public override void Render(IRenderConfiguration renderConfig, RenderManager renderManager, IMaterial materialToUse)
        {
            base.Render(renderConfig, renderManager, materialToUse);

            Performance.StartAppendAggreate("Setup");
            Matrix worldViewProj = WorldMatrix * renderManager.ViewTarget.ViewMatrix * renderManager.ViewTarget.ProjectionMatrix;
            materialToUse.ApplyParameters(worldViewProj);
            Performance.EndAppendAggreate("Setup");

            Performance.StartAppendAggreate("Render Mesh");
            //SkeletalMesh.Render();
            Performance.EndAppendAggreate("Render Mesh");

            /*SkeletalMesh.SetAnimationTime(AnimationTime);
            for (int i = 0; i < SkeletalMesh.MeshParts.Count; i++)
            {
                RenderMeshPart(SkeletalMesh.MeshParts[i], renderManager, renderConfig, materialToUse);
            }

            for (int i = 0; i < SkeletalMesh.RootBone.ChildBones.Length; i++)
            {
                //DrawBone(SkeletalMesh.RootBone.ChildBones[i], renderManager, renderConfig);
            }*/
        }

        internal override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            lock (lockObj)
            {
                AnimationTime += nextAnimationTimeDelta;
            }
        }

        public void TickAnimation(double deltaTime)
        {
            lock (lockObj)
            {
                nextAnimationTimeDelta = deltaTime;
            }
        }
    }
}
