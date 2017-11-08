using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlkaronEngine.Graphics2D
{
   public interface IRenderConfiguration
   {
      GraphicsDevice GraphicsDevice { get; }

      Vector2 ScreenSize { get; }

      Vector2 Scale { get; }
      Vector2 ScaledOffset { get; }

      bool RequiresPowerOfTwoTextures { get; }

      PrimitiveRenderManager RenderManager { get; }
   }
}
