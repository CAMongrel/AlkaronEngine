using System;
using System.IO;
using System.Linq;
using AlkaronEngine.Assets.Materials;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Assets.Importers
{
    public static class AssetImporterSurface2D
    {
        private static readonly string[] allowedExtensions = new string[] { ".png", ".jpg" };

        #region GetNiceInfo
        /// <summary>
        /// Returns a nicely formatted string with information about the texture
        /// </summary>
        static string GetNiceInfo(Texture2D texture)
        {
            return
                "Texture2D information:\r\n" +
                "==============================\r\n" +
                "Width: " + texture.Width + "\r\n" +
                "Height: " + texture.Height + "\r\n" +
                "MipLevels: " + texture.LevelCount + "\r\n" +
                "Format: " + texture.Format;
        }
        #endregion

        #region Import
        /// <summary>
        /// Imports a new surface 2D into the content system
        /// </summary>
        public static bool Import(string fullFilename,
            string setAssetName,
            string setPackageName,
            out Surface2D importedAsset)
        {
            importedAsset = null;

            // Remember input filename
            string inputFile = fullFilename;
            if (File.Exists(inputFile) == false)
            {
                AlkaronCoreGame.Core.Log("Import file '" + inputFile + "' is not valid!");
                return false;
            }

            string extension = Path.GetExtension(inputFile);
            if (string.IsNullOrWhiteSpace(extension))
            {
                AlkaronCoreGame.Core.Log("Import file '" + inputFile + "' has an invalid file extension!");
                return false;
            }

            extension = extension.ToLowerInvariant();
            if (allowedExtensions.Contains(extension) == false)
            {
                AlkaronCoreGame.Core.Log("Import file '" + inputFile + "' has an invalid file extension!");
                return false;
            }

            using (FileStream fs = File.OpenRead(inputFile))
            {
                return Import(fs, setAssetName, setPackageName, inputFile, out importedAsset);
            }
        }

        /// <summary>
        /// Imports a new surface 2D into the content system
        /// </summary>
        public static bool Import(Stream surfaceStream,
            string setAssetName,
            string setPackageName,
            string originalInputFile,
            out Surface2D importedAsset)
        {
            if (originalInputFile == null)
            {
                throw new ArgumentNullException(nameof(originalInputFile));
            }

            importedAsset = null;

            // Create asset and package names
            string assetName = setAssetName;
            string packageName = setPackageName;

            Package packageToSaveIn = null;

            assetName = Path.ChangeExtension(assetName, ".surface2d");

            if (AlkaronCoreGame.Core.PackageManager.DoesPackageExist(packageName))
            {
                packageToSaveIn = AlkaronCoreGame.Core.PackageManager.LoadPackage(packageName, false);
            }
            else
            {
                packageToSaveIn = AlkaronCoreGame.Core.PackageManager.CreatePackage(packageName,
                    Path.Combine(AlkaronCoreGame.Core.ContentDirectory, packageName));
            }

            if (packageToSaveIn == null)
            {
                AlkaronCoreGame.Core.Log("Unable to create or find the package for this asset");
                return false;
            }

            try
            {
                // Import existing file and convert it to the new format
                Texture2D newTex = Texture2D.FromStream(AlkaronCoreGame.Core.GraphicsDevice, surfaceStream);

                Surface2D surface2D = new Surface2D(newTex);
                surface2D.OriginalFilename = originalInputFile;
                surface2D.Name = assetName;

                packageToSaveIn.StoreAsset(surface2D);
            }
            catch (Exception ex)
            {
                AlkaronCoreGame.Core.Log("Failed to import Surface2D: " + ex.ToString());
                return false;
            }

            return true;
        }
        #endregion
    }
}
