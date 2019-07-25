using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Util;
using System.Numerics;

namespace AlkaronEngine.Graphics3D.RenderProxies
{
    internal class StaticMeshRenderProxy : BaseRenderProxy
    {
        public StaticMesh StaticMesh { get; private set; }

        public StaticMeshRenderProxy(StaticMesh setStaticMesh)
           : base()
        {
            Type = RenderProxyType.SkeletalMesh;
            StaticMesh = setStaticMesh;
            Material = StaticMesh.Material;
            if (Material == null)
            {
                Material = new Material();
            }
        }

        public override void Render(RenderContext renderContext, IMaterial materialToUse)
        {
            /*if (StaticMesh.IsCollisionOnly)
            {
                return;
            }*/

            base.Render(renderContext, materialToUse);

            Performance.StartAppendAggreate("Setup");
            materialToUse.ApplyParameters(renderContext, WorldMatrix);
            Performance.EndAppendAggreate("Setup");

            /*if (StaticMesh.DiffuseTexture != null)
            {
                Performance.StartAppendAggreate("Setup Texture");
                materialToUse.Effect.Parameters["Texture"].SetValue(StaticMesh.DiffuseTexture);
                materialToUse.Effect.CurrentTechnique.Passes[0].Apply();
                Performance.EndAppendAggreate("Setup Texture");
            }*/

            Performance.StartAppendAggreate("Render Mesh");
            StaticMesh.Render(renderContext);
            Performance.EndAppendAggreate("Render Mesh");

            renderContext.RenderedTrianglesThisFrame += StaticMesh.NumberOfFaces;
        }
    }
}
