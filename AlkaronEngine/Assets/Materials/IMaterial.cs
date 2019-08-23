using AlkaronEngine.Graphics3D;
using System.Numerics;

namespace AlkaronEngine.Assets.Materials
{
    public interface IMaterial
    {
        bool RequiresOrderingBackToFront { get; set; }

        void ApplyParameters(RenderContext renderContext, Matrix4x4 worldMatrix, Matrix4x4[]? boneMatrices = null);
        void SetupMaterialForRenderPass(RenderContext renderContext);
    }
}
