using System;
using System.Collections.Generic;

namespace AlkaronEngine.Assets.Importers
{
    public static class AssetImporterSkeletalMesh
    {
        private static readonly string[] allowedExtensions = new string[] { ".gltf", ".glb" };

        public static bool Import(string fullFilename,
            string setAssetName,
            string setPackageName,
            out List<Asset> importedAssets)
        {
            importedAssets = new List<Asset>();

            return true;
        }
    }
}