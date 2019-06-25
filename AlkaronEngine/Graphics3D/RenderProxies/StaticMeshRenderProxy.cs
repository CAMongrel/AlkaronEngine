using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Util;
using System.Numerics;

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

        public override void Render()//Graphics2D.IRenderConfiguration renderConfig, RenderManager renderManager, IMaterial materialToUse)
        {
            /*if (StaticMesh.IsCollisionOnly)
            {
                return;
            }*/

            base.Render();//renderConfig, renderManager, materialToUse);

            Performance.StartAppendAggreate("Setup");
            //Matrix4x4 worldViewProj = WorldMatrix * renderManager.ViewTarget.ViewMatrix * renderManager.ViewTarget.ProjectionMatrix;
            //materialToUse.ApplyParameters(worldViewProj);
            Performance.EndAppendAggreate("Setup");

            /*if (StaticMesh.DiffuseTexture != null)
            {
                Performance.StartAppendAggreate("Setup Texture");
                materialToUse.Effect.Parameters["Texture"].SetValue(StaticMesh.DiffuseTexture);
                materialToUse.Effect.CurrentTechnique.Passes[0].Apply();
                Performance.EndAppendAggreate("Setup Texture");
            }*/

            Performance.StartAppendAggreate("Render Mesh");
            StaticMesh.Render();
            Performance.EndAppendAggreate("Render Mesh");
        }
    }
}
