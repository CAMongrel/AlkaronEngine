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

        #region GetPixelStride
        /// <summary>
        /// Returns the size in bytes of a single texel 
        /// </summary>
        static float GetPixelStride(SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Alpha8:
                    return 1;

                case SurfaceFormat.Bgr565:
                    return 2;

                case SurfaceFormat.Color:
                    return 4;

                case SurfaceFormat.Single:
                    return 4;

                case SurfaceFormat.Dxt1:
                    return 0.5f;

                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                    return 1;

                default:
                    return -1;
            }
        }
        #endregion

        #region IsCompressedFormat
        /// <summary>
        /// Checks if the format is DXT compressed
        /// </summary>
        static bool IsCompressedFormat(SurfaceFormat format)
        {
            switch (format)
            {
                default:
                    return false;

                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                    return true;
            }
        }
        #endregion

        #region Import
        /// <summary>
        /// Imports a new surface 2D into the content system
        /// </summary>
        public static bool Import(string fullFilename,
            string setAssetName,
            string setPackageName,
            Asset existingAsset,
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
                return Import(fs, setAssetName, setPackageName, inputFile, existingAsset, out importedAsset);
            }
        }

        /// <summary>
        /// Imports a new surface 2D into the content system
        /// </summary>
        public static bool Import(Stream surfaceStream,
            string setAssetName,
            string setPackageName,
            string originalInputFile,
            Asset existingAsset,
            out Surface2D importedAsset)
        {
            importedAsset = null;

            // Create asset and package names
            string assetName = setAssetName;
            string packageName = setPackageName;

            Package packageToSaveIn = null;

            assetName = Path.ChangeExtension(assetName, ".surface2d");

            if (AlkaronCoreGame.Core.PackageManager.DoesPackageExist(packageName))
            {
                packageToSaveIn = AlkaronCoreGame.Core.PackageManager.LoadPackage(packageName);
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

                using (newTex)
                {
                    // Save imported asset
                    using (MemoryStream memStr = new MemoryStream())
                    {
                        using (BinaryWriter writer = new BinaryWriter(memStr))
                        {
                            writer.Write("HAF ");
                            writer.Write(Surface2D.MaxAssetVersion);
                            writer.Write(originalInputFile);

                            writer.Write(newTex.Width);
                            writer.Write(newTex.Height);
                            writer.Write(newTex.LevelCount);
                            writer.Write((int)newTex.Format);

                            int[] mipStart = new int[newTex.LevelCount];
                            int[] mipSize = new int[newTex.LevelCount];

                            int texelCount = 0;
                            int mipWidth = newTex.Width;
                            int mipHeight = newTex.Height;
                            for (int i = 0; i < newTex.LevelCount; i++)
                            {
                                mipStart[i] = texelCount;
                                mipSize[i] = (int)((float)mipWidth * (float)mipHeight *
                                    GetPixelStride(newTex.Format));

                                texelCount += mipSize[i];

                                mipWidth /= 2;
                                mipHeight /= 2;

                                if (IsCompressedFormat(newTex.Format))
                                {
                                    if (mipWidth < 4)
                                    {
                                        mipWidth = 4;
                                    }
                                    if (mipHeight < 4)
                                    {
                                        mipHeight = 4;
                                    }
                                }
                                else
                                {
                                    if (mipWidth < 1)
                                    {
                                        mipWidth = 1;
                                    }
                                    if (mipHeight < 1)
                                    {
                                        mipHeight = 1;
                                    }
                                }
                            }
                            byte[] data = new byte[texelCount];

                            for (int i = 0; i < newTex.LevelCount; i++)
                            {
                                newTex.GetData<byte>(i, null, data, mipStart[i], mipSize[i]);
                            }

                            writer.Write(texelCount);
                            writer.Write(data);

                            for (int i = 0; i < newTex.LevelCount; i++)
                            {
                                writer.Write(mipStart[i]);
                                writer.Write(mipSize[i]);
                            }

                            // Rewind stream
                            memStr.Position = 0;

                            // Re-read asset
                            Surface2D surf = null;
                            if (existingAsset == null)
                            {
                                surf = new Surface2D();
                            }
                            else
                            {
                                surf = existingAsset as Surface2D;
                            }
                            surf.Load(packageName, assetName, memStr);
                            packageToSaveIn.StoreAsset(assetName, surf);

                            importedAsset = surf;
                        }
                    }
                }
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
