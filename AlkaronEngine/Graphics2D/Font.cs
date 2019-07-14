using AlkaronEngine.Assets.Materials;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlkaronEngine.Graphics2D
{
    class Font
    {
        internal Surface2D FontSurface { get; private set; }

        public Font(Surface2D fontTexture, float glyphWdith, float glyphHeight)
        {
            FontSurface = fontTexture;
        }
    }
}
