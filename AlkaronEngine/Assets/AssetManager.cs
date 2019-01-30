using System.Collections.Generic;
using System.IO;

namespace AlkaronEngine.Assets
{
	public class AssetManager
	{
		#region Load<T>
		public T Load<T>(string FullAssetName) where T : Asset, new()
		{
			if (FullAssetName == null)
            {
                return null;
            }

            string[] assetPath = FullAssetName.Split('.');
			if (assetPath.Length != 3)
			{
				// TODO!!!
				//Log.LogError("Invalid name specified for Load Asset.");
				return new T();
			}
			
			Package pkg = AlkaronCoreGame.Core.PackageManager.LoadPackage(assetPath[0]);
			if (pkg == null)
            {
                return new T();
            }

            T asset = pkg.GetAsset(assetPath[1] + "." + assetPath[2]) as T;
			if (asset == null)
            {
                return new T();
            }

            return asset;
		}
		#endregion		

		#region GetAssetNamesByType
		/// <summary>
		/// Returns a list of all assets of the specified type (e.g. "Surface2D")
		/// in the package specified by "packageToSearch".
		/// "packageToSearch" can be null in which case all known/loaded packages
		/// will be searched.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="packageToSearch"></param>
		/// <returns></returns>
		public string[] GetAssetNamesByType(string type, 
			Package packageToSearch, bool excludePackage)
		{
			if (packageToSearch != null &&
				excludePackage == false)
            {
                return InternalGetAssetNamesByType(type, packageToSearch);
            }

            List<string> resultList = new List<string>();
			foreach (KeyValuePair<string, Package> pair in AlkaronCoreGame.Core.PackageManager.LoadedPackages)
			{
				if (pair.Value == packageToSearch)
                {
                    continue;
                }

                resultList.AddRange(InternalGetAssetNamesByType(
					type, pair.Value));
			}

			resultList.Sort();

			return resultList.ToArray();
		}

		/// <summary>
		/// Internal helper for GetAssetNamesByType.
		/// This function does the actual searching of a package.
		/// </summary>
		private static string[] InternalGetAssetNamesByType(string type,
			Package packageToSearch)
		{
			List<string> resultList = new List<string>();

			Asset[] assetList = packageToSearch.GetAssetsByType(type);
			for (int i = 0; i < assetList.Length; i++)
			{
				resultList.Add(assetList[i].Fullname);
			}

			return resultList.ToArray();
		}
		#endregion

		#region GetOriginalFilename
		/// <summary>
		/// Returns the original filename of the asset from which it
		/// was imported. Returns null if any error happens.
		/// </summary>
		public static string GetOriginalFilename(string AssetName)
		{
			string fullAssetname = Path.Combine(
				AlkaronCoreGame.Core.ContentDirectory, AssetName);

			if (File.Exists(fullAssetname) == false)
            {
                return null;
            }

            string result = null;
			using (BinaryReader reader = 
				new BinaryReader(File.OpenRead(fullAssetname)))
			{
				try
				{
					result = reader.ReadString();
				}
				catch
				{
					result = null;
				}
			}

			if (result == null ||
				File.Exists(result) == false)
            {
                return null;
            }

            return result;
		}
		#endregion
	}
}
