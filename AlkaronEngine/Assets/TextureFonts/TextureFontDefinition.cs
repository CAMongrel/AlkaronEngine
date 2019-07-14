using System;
using System.Collections.Generic;
using System.Text;

namespace AlkaronEngine.Assets.TextureFonts
{
    public class CharacterDefinition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int OriginX { get; set; }
        public int OriginY { get; set; }
        public int Advance { get; set; }
    }

    public class TextureFontDefinition
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Dictionary<string, CharacterDefinition> Characters;
    }
}
