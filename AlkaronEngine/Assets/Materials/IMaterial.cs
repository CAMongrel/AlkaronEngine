using AlkaronEngine.Graphics3D;
using System.Numerics;

namespace AlkaronEngine.Assets.Materials
{
    public interface IMaterial
    {
        bool RequiresOrderingBackToFront { get; set; }

        void ApplyParameters(RenderContext renderContext, Matrix4x4 worldViewProjectio);
        void SetupMaterialForRenderPass(RenderContext renderContext, RenderPass renderPass);
    }
}
