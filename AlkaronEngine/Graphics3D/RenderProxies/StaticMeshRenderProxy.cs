using System;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
    public class StaticMeshRenderProxy : BaseRenderProxy
    {
        public StaticMesh StaticMesh { get; private set; }

        public StaticMeshRenderProxy(StaticMesh setStaticMesh)
           : base()
        {
            StaticMesh = setStaticMesh;
            Material = StaticMesh.Material;
        }

        public override void Render(Graphics2D.IRenderConfiguration renderConfig, RenderManager renderManager)
        {
            if (StaticMesh.IsCollisionOnly)
            {
                return;
            }

            base.Render(renderConfig, renderManager);

            Performance.StartAppendAggreate("Setup");

            renderConfig.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            renderConfig.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

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
            Performance.EndAppendAggreate("DrawPrimitives");
        }
    }
}
