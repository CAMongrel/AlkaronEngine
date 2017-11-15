using System;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D.Components;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
   public class StaticMeshComponentRenderProxy : BaseRenderProxy
   {
      public StaticMeshComponent Component { get; set; }

      public StaticMeshComponentRenderProxy(Effect setEffect, 
                                            StaticMeshComponent setComponent)
         : base(setEffect)
      {
         Component = setComponent;
      }

      public override void Render(IRenderConfiguration renderConfig, RenderManager renderManager)
      {
         base.Render(renderConfig, renderManager);

         Performance.StartAppendAggreate("Setup");
         renderConfig.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
         renderConfig.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

         Effect.Parameters["WorldViewProj"].SetValue(WorldMatrix * renderManager.ViewMatrix * renderManager.ProjectionMatrix);
         Effect.CurrentTechnique.Passes[0].Apply();
         Performance.EndAppendAggreate("Setup");

         for (int i = 0; i < Component.StaticMeshes.Count; i++)
         {
            StaticMesh mesh = Component.StaticMeshes[i];
            if (mesh.IsCollisionOnly)
            {
               continue;
            }

            if (mesh.DiffuseTexture != null)
            {
               Performance.StartAppendAggreate("Setup");
               Effect.Parameters["Texture"].SetValue(mesh.DiffuseTexture);
               Effect.CurrentTechnique.Passes[0].Apply();
               Performance.EndAppendAggreate("Setup");
            }

            Performance.StartAppendAggreate("SetVertexBuffer");
            renderConfig.GraphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
            Performance.EndAppendAggreate("SetVertexBuffer");
            Performance.StartAppendAggreate("DrawPrimitives");
            renderConfig.GraphicsDevice.DrawPrimitives(mesh.PrimitiveType, 0, mesh.PrimitiveCount);
            Performance.EndAppendAggreate("DrawPrimitives");
         }
      }
   }
}
