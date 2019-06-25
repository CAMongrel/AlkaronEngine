using System.Numerics;

namespace AlkaronEngine.Assets.Materials
{
    public interface IMaterial
    {
        bool RequiresOrderingBackToFront { get; set; }

        void ApplyParameters(Matrix4x4 worldViewProjectio);
        //void SetupEffectForRenderPass(RenderPass renderPass);
    }
}
