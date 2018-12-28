using AlkaronEngine.Scene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlkaronEngine.Graphics2D
{
    public class PrimitiveRenderManager
    {
        private IRenderConfiguration renderConfig;

        public SpriteBatch SpriteBatch { get; private set; }

        public SpriteFont EngineFont { get; set; }

        public PrimitiveRenderManager(IRenderConfiguration setRenderConfig)
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
