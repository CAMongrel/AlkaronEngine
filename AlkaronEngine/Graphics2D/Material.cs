using System;
using AlkaronEngine.Graphics3D;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics2D
{
    public class Material
    {
        public Effect Effect { get; protected set; }

        public bool RequiresOrderingBackToFront { get; set; }

        public IRenderConfiguration RenderConfig { get; private set; }

        public BlendState BlendState { get; set; }

        public Material(IRenderConfiguration renderConfig)
        {
            RenderConfig = renderConfig;
        }

        public void SetEffect(Effect newEffect)
        {
            Effect = newEffect;
        }

        public virtual void SetupEffectForRenderPass(RenderPass renderPass)
        {
            RenderConfig.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            RenderConfig.GraphicsDevice.BlendState = BlendState;
        }
    }
}
