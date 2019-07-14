using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
            FontDefinition = null;
            Surface = null;
        }

        internal TextureFont(Surface2D setSurface, TextureFontDefinition setFontDefinition)
        {
            FontDefinition = setFontDefinition;
            Surface = setSurface;
        }

        internal override void Serialize(BinaryWriter writer, AssetSettings assetSettings)
        {
            base.Serialize(writer, assetSettings);

            writer.Write(Surface);

            writer.Write(JsonConvert.SerializeObject(FontDefinition));
        }

        internal override void Deserialize(BinaryReader reader, AssetSettings assetSettings)
        {
            base.Deserialize(reader, assetSettings);

            var surface = reader.ReadAsset<Surface2D>();
            if (surface == null)
            {
                throw new InvalidDataException("Could not read referenced surface asset for TextureFont");
            }

            Surface = surface;

            string json = reader.ReadString();
            FontDefinition = JsonConvert.DeserializeObject<TextureFontDefinition>(json);
        }
    }
}
