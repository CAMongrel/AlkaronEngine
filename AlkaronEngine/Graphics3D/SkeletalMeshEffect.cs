using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
    public class SkeletalMeshEffect : Effect
    {
        public SkeletalMeshEffect(IRenderConfiguration renderConfig, byte[] effectCode)
            : base(renderConfig.GraphicsDevice, effectCode)
        {
            
        }
    }
}
