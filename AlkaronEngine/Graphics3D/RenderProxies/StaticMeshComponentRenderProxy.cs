using System;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D.Components;
using AlkaronEngine.Graphics3D.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
   public class StaticMeshComponentRenderProxy : BaseRenderProxy
   {
      public StaticMeshComponent Component { get; set; }
      public Matrix WorldMatrix { get; set; }

      public StaticMeshComponentRenderProxy(Effect setEffect, 
                                           StaticMeshComponent setComponent)
         : base(setEffect)
      {
         Component = setComponent;
         WorldMatrix = Matrix.Identity;
      }

      public override void Render(IRenderConfiguration renderConfig, RenderManager renderManager)
      {
         base.Render(renderConfig, renderManager);

         renderConfig.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

         Effect.Parameters["WorldViewProj"].SetValue(WorldMatrix * renderManager.ViewMatrix * renderManager.ProjectionMatrix);
         Effect.CurrentTechnique.Passes[0].Apply();

         for (int i = 0; i < Component.StaticMeshes.Count; i++)
         {
            StaticMesh mesh = Component.StaticMeshes[i];

            if (mesh.DiffuseTexture != null)
            {
               Effect.Parameters["Texture"].SetValue(mesh.DiffuseTexture);
               Effect.CurrentTechnique.Passes[0].Apply();
            }

            renderConfig.GraphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
            renderConfig.GraphicsDevice.DrawPrimitives(mesh.PrimitiveType, 0, mesh.PrimitiveCount);
         }
      }
   }
}
