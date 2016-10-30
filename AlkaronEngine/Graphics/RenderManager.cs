﻿using AlkaronEngine.Scene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlkaronEngine.Graphics
{
   public class RenderManager
   {
      private IRenderConfiguration renderConfig;

      public SpriteBatch SpriteBatch { get; private set; }

      internal RenderManager(IRenderConfiguration setRenderConfig)
      {
         if (setRenderConfig == null)
         {
            throw new ArgumentNullException(nameof(setRenderConfig));
         }
         renderConfig = setRenderConfig;

         SpriteBatch = new SpriteBatch(renderConfig.GraphicsDevice);
      }
   }
}
