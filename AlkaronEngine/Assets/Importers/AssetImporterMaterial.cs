using System;
using AlkaronEngine.Assets.Materials;

namespace AlkaronEngine.Assets.Importers
{
    public class AssetImporterMaterial
    {
        private static readonly string[] allowedExtensions = new string[] { ".mgfxo", ".glb" };

        public static bool Import(string fullFilename,
            string setAssetName,
            string setPackageName,
            out Material importedAsset)
        {
            importedAsset = null;

            return true;
        }
    }
}
