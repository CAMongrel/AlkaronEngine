using System;
using AlkaronEngine.Graphics3D;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Assets.Materials
{
    public interface IMaterial
    {
        bool RequiresOrderingBackToFront { get; set; }

        void ApplyParameters(Matrix worldViewProjectio);
        void SetupEffectForRenderPass(RenderPass renderPass);
    }
}
