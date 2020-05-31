using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Assets.TextureFonts;
using AlkaronEngine.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AlkaronEngine.Assets.Importers
{
    class AssetImporterTextureFont
    {
        private static readonly string[] allowedExtensions = new string[] { ".json" };

        public static bool Import(Surface2D setSurface,
            string setFontDefinitionFile,
            string setAssetName,
            string setPackageName,
            AssetSettings assetSettings,
            out TextureFont? importedAsset)
        {
            importedAsset = null;

            // Remember input filename
            string inputFile = setFontDefinitionFile;
            if (File.Exists(inputFile) == false)
            {
                Log.Status("Import file '" + inputFile + "' is not valid!");
                return false;
            }

            string extension = Path.GetExtension(inputFile);
            if (string.IsNullOrWhiteSpace(extension))
            {
                Log.Status("Import file '" + inputFile + "' has an invalid file extension!");
                return false;
            }

            extension = extension.ToLowerInvariant();
            if (allowedExtensions.Contains(extension) == false)
            {
                Log.Status("Import file '" + inputFile + "' has an invalid file extension!");
                return false;
            }

            // Create asset and package names
            string assetName = setAssetName;
            string packageName = setPackageName;

            Package? packageToSaveIn = null;

            if (string.IsNullOrWhiteSpace(assetName))
            {
                assetName = "temporaryTextureFont";
                if (string.IsNullOrWhiteSpace(inputFile))
                {
                    assetName += "_" + DateTimeOffset.UtcNow.UtcTicks;
                }
                else
                {
                    assetName += "_" + Path.GetFileNameWithoutExtension(inputFile);
                }
            }

            assetName = Path.ChangeExtension(assetName, ".textureFont");

            if (packageName != null)
            {
                if (AlkaronCoreGame.Core.PackageManager.DoesPackageExist(packageName))
                {
                    packageToSaveIn = AlkaronCoreGame.Core.PackageManager.LoadPackage(packageName, false, assetSettings);
                }
                else
                {
                    packageToSaveIn = AlkaronCoreGame.Core.PackageManager.CreatePackage(
                        packageName,
                        Path.Combine(AlkaronCoreGame.Core.ContentDirectory, packageName),
                        assetSettings);
                }
            }
            else
            {
                packageToSaveIn = AlkaronCoreGame.Core.PackageManager.LoadPackage(PackageManager.VolatilePackageName, false, assetSettings);
            }

            if (packageToSaveIn == null)
            {
                Log.Status("Unable to create or find the package for this asset");
                return false;
            }

            try
            {
                TextureFontDefinition tfd = Newtonsoft.Json.JsonConvert.DeserializeObject<TextureFontDefinition>(File.ReadAllText(inputFile));

                TextureFont tf = new TextureFont(setSurface, tfd);
                tf.OriginalFilename = inputFile;
                tf.Name = assetName;

                packageToSaveIn.StoreAsset(tf);

                importedAsset = tf;
            }
            catch (Exception ex)
            {
                Log.Status("Failed to import TextureFont: " + ex.ToString());
                return false;
            }

            return true;
        }
    }
}
