using AlkaronEngine.Assets.Materials;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlkaronEngine.Assets.TextureFonts
{
    public class TextureFont : Asset
    {
        public TextureFontDefinition FontDefinition { get; private set; }

        public Surface2D Surface { get; private set; }

        public override bool IsValid => FontDefinition != null && Surface != null && Surface.IsValid;

        public TextureFont()
        {

        }
    }
}
