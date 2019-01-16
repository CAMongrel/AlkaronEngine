using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.Geometry
{
    public class SkeletalMeshMaterial : Material
    {
        public SkeletalMeshMaterial(IRenderConfiguration renderConfig)
            : base(renderConfig)
        {
            Effect = new SkinnedEffect(renderConfig.GraphicsDevice);
            BlendState = BlendState.NonPremultiplied;
        }
    }
}
