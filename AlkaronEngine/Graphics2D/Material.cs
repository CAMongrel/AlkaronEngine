using System;
using AlkaronEngine.Graphics3D;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics2D
{
   public class Material
   {
      public Effect Effect { get; protected set; }

      public bool RequiresOrderingBackToFront { get; set; }

      public Material()
      {
      }

      public void SetEffect(Effect newEffect)
      {
         Effect = newEffect;
      }

      public virtual void SetupEffectForRenderPass(RenderPass renderPass)
      {

      }
   }
}
