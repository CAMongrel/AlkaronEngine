using System;
using System.IO;
using System.Linq;
using AlkaronEngine.Assets.Materials;

namespace AlkaronEngine.Assets.Importers
{
    public class AssetImporterMaterial
    {
        private static readonly string[] allowedExtensions = new string[] { ".mgfxo" };

        public static bool Import(string fullFilename,
            string setAssetName,
            string setPackageName,
            out Material importedAsset)
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

            // Create asset and package names
            string assetName = setAssetName;
            string packageName = setPackageName;

            Package packageToSaveIn = null;

            assetName = Path.ChangeExtension(assetName, ".material");

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

            // Create the actual input file names
            string dx11Filename = GetGraphicsProfileFilename("dx11", fullFilename);
            string oglFilename = GetGraphicsProfileFilename("ogl", fullFilename);

            try
            {
                Material mat = new Material();
                mat.OriginalFilename = dx11Filename ?? (oglFilename ?? string.Empty);
                mat.Name = assetName;

                if (dx11Filename != null)
                {
                    byte[] allBytes = File.ReadAllBytes(dx11Filename);
                    mat.AddBinaryCode(GraphicsLibrary.DirectX11, Material.CodeType.XNAEffect,
                        allBytes);
                }
                if (oglFilename != null)
                {
                    byte[] allBytes = File.ReadAllBytes(oglFilename);
                    mat.AddBinaryCode(GraphicsLibrary.OpenGL, Material.CodeType.XNAEffect,
                        allBytes);
                }

                packageToSaveIn.StoreAsset(mat);
            }
            catch (Exception ex)
            {
                AlkaronCoreGame.Core.Log("Failed to import Mesh: " + ex);
                return false;
            }

            return true;
        }

        private static string GetGraphicsProfileFilename(string profile, string fullFilename)
        {
            // Sample: BasicEffect.dx11.mgfxo, BasicEffect.ogl.mgfxo

            string path = Path.GetDirectoryName(fullFilename);
            string extension = Path.GetExtension(fullFilename);
            string filename = Path.GetFileName(fullFilename).ToLowerInvariant();
            string filenameWOExtension = Path.GetFileNameWithoutExtension(filename);

            if (filenameWOExtension.EndsWith("dx11", StringComparison.InvariantCultureIgnoreCase) ||
                filenameWOExtension.EndsWith("ogl", StringComparison.InvariantCultureIgnoreCase))
            {
                // The full filename already contained a platform, replace it with the profile
                return Path.Combine(path, Path.ChangeExtension(filenameWOExtension, "." + profile)) + extension;
            }
            else
            {
                // Just append the profile to the filename
                return Path.Combine(path, filenameWOExtension) + "." + profile + extension;
            }
        }
    }
}
