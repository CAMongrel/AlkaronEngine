using System.Collections.Generic;
using System.IO;
using Veldrid;

namespace AlkaronEngine.Assets
{
	/// <summary>
	/// Class managing a list of all loaded packages.
	/// </summary>
	public class PackageManager
	{
		/// <summary>
		/// The PackageMap contains a list of ALL packages on the hard disk
		/// inside the package directory. These packages are not neccessarily
		/// loaded yet, though.
		/// </summary>
		internal Dictionary<string, string> PackageMap;
		/// <summary>
		/// A list of all fully loaded packages.
		/// </summary>
		internal Dictionary<string, Package> LoadedPackages;
		
		#region PackageManager
		public PackageManager()
		{
			PackageMap = new Dictionary<string, string>();
			LoadedPackages = new Dictionary<string, Package>();
			
			Package volatilePkg = Package.CreateVolatile();
			LoadedPackages.Add(Package.VolatilePackageName + ".package", volatilePkg);
		}
		#endregion
		
		#region BuildPackageMap
		/// <summary>
		/// Builds the package map containing links from the Package
		/// to the location on disk.
		/// </summary>
		public void BuildPackageMap()
		{
            // Load packages from embedded resources first
            // Implementation note:
            // This prevents packages with the same name being loaded from
            // disk, which is important for engine related packages like
            // EngineMaterials.package, etc.
            string[] embeddedPackages = AlkaronCoreGame.Core.AlkaronContent.GetResourcesByType(
                ".package");
            for (int i = 0; i < embeddedPackages.Length; i++)
            {
                string packageName = AlkaronContentManager.GetResourceFilename(embeddedPackages[i]);

                if (PackageMap.ContainsKey(packageName))
                {
                    AlkaronCoreGame.Core.Log("Found duplicate package '" +
                        packageName + "' at \r\n" +
                        embeddedPackages[i] + "\r\nUsing the already loaded package at\r\n" +
                        PackageMap[packageName]);

                    continue;
                }

                PackageMap.Add(packageName, embeddedPackages[i]);
            }

            // Load packages from disk
            string[] packages = Directory.GetFiles(
                AlkaronCoreGame.Core.ContentDirectory,
				"*.package", SearchOption.AllDirectories);

			for (int i = 0; i < packages.Length; i++)
			{
				string packageName = Path.GetFileName(packages[i]);
				
				if (PackageMap.ContainsKey(packageName))
				{
                    AlkaronCoreGame.Core.Log("Found duplicate package '" + 
						packageName + "' at \r\n" +
						packages[i] + "\r\nUsing the already loaded package at\r\n" +
						PackageMap[packageName]);
					
					continue;
				}
				
				PackageMap.Add(packageName, packages[i]);
			}
		}
		#endregion
	
		#region LoadPackage
		/// <summary>
		/// Loads a package from disk or returns an already loaded
		/// package.
        /// Loads all assets of the package into memory, if "loadFully" is 
        /// <see langword="true"/>.
		/// </summary>
		public Package LoadPackage(string packageName, bool loadFully, AssetSettings assetSettings)
		{
			packageName = Path.ChangeExtension(packageName, ".package");
			
			if (LoadedPackages.ContainsKey(packageName))
            {
                return LoadedPackages[packageName];
            }

            string filename = null;
			if (PackageMap.ContainsKey(packageName))
            {
                filename = PackageMap[packageName];
            }

            if (filename == null)
			{
                AlkaronCoreGame.Core.Log("Trying to open nonexistent package '" + 
					packageName + "'");
				return null;
			}

            bool isResource = AlkaronCoreGame.Core.AlkaronContent.IsResource(filename);

            Package pkg = null;
            Stream stream = null;
            if (isResource)
            {
                stream = AlkaronCoreGame.Core.AlkaronContent.OpenResourceStream(filename);
                pkg = new Package(stream, assetSettings, true);
                loadFully = true;
            }
            else
            {
                pkg = new Package(filename, assetSettings);
            }
			LoadedPackages.Add(packageName, pkg);
            if (loadFully)
            {
                if (isResource)
                {
                    // Reopen the stream, because it was previously closed in the Package ctor
                    stream = AlkaronCoreGame.Core.AlkaronContent.OpenResourceStream(filename);
                }
                pkg.LoadAllAssets(assetSettings, stream);
            }

#if IS_EDITOR
			if (Program.Game.IsEditor)
				RefreshPackageNodeList();
#endif

            return pkg;
		}
		#endregion
		
		#region DoesPackageExist
		/// <summary>
		/// Checks if the specified package exists on disk and is known
		/// to the PackageManager
		/// </summary>
		public bool DoesPackageExist(string packageName)
		{
			packageName = Path.ChangeExtension(packageName, ".package");
			return PackageMap.ContainsKey(packageName);
		}
		#endregion
		
		#region Cleanup
		/// <summary>
		/// Disposes all loaded packages and lets them clear their loaded
		/// assets.
		/// </summary>
		public void Cleanup()
		{
			foreach (var pair in LoadedPackages)
			{
				pair.Value.Dispose();
			}
			LoadedPackages.Clear();
		}
        #endregion

#if IS_EDITOR
        #region RefreshPackageNodeList
		/// <summary>
		/// Refreshes the "Is Loaded" status of the package nodes.
		/// </summary>
		private static void RefreshPackageNodeList()
		{
			TreeNode baseNode = Program.Game.GetPackageListRootNode();

			for (int i = 0; i < baseNode.Nodes.Count; i++)
			{
				bool IsLoadedPackage = LoadedPackages.ContainsKey(
					baseNode.Nodes[i].Text);

				if (IsLoadedPackage)
				{
					baseNode.Nodes[i].NodeFont = new System.Drawing.Font(
						baseNode.Nodes[i].TreeView.Font,
						System.Drawing.FontStyle.Bold);

					baseNode.Nodes[i].Text = baseNode.Nodes[i].Text;
				}
			}
		}
        #endregion

        #region FillNodeList
		/// <summary>
		/// Fills the given TreeNode with a list of all known packages.
		/// Already loaded packages will be in bold.
		/// </summary>
		public static void FillNodeList()
		{
			TreeNode baseNode = Program.Game.GetPackageListRootNode();

			foreach (KeyValuePair<string, string> pair in PackageMap)
			{
				bool IsLoadedPackage = LoadedPackages.ContainsKey(pair.Key);

				TreeNode node = new TreeNode(pair.Key);
				node.Tag = null;
				if (IsLoadedPackage)
				{
					System.Drawing.Font font2Use = node.NodeFont;
					if (font2Use == null)
						font2Use = baseNode.TreeView.Font;

					node.NodeFont = new System.Drawing.Font(font2Use,
						System.Drawing.FontStyle.Bold);
					node.Tag = LoadedPackages[pair.Key];
				}
				baseNode.Nodes.Add(node);
			}
		}
        #endregion
#endif

        #region CreatePackage
        /// <summary>
        /// Creates a new package or opens an existing package of the 
        /// same name. 
        /// Uses "targetFilename" to store the newly created package on disk.
        /// TargetFilename *should* be a subdirectory of 
        /// AlkaronCoreGame.ContentDirectory.
        /// </summary>
        public Package CreatePackage(string packageName, string targetFilename, AssetSettings assetSettings)
		{
			if (DoesPackageExist(packageName))
            {
                // Log warning?
                return LoadPackage(packageName, false, assetSettings);
            }

            packageName = Path.ChangeExtension(packageName, ".package");

            targetFilename = Path.ChangeExtension(targetFilename, ".package");
            string fullFilename = targetFilename;

			Package pkg = new Package(fullFilename, assetSettings);
			PackageMap.Add(packageName, fullFilename);
			LoadedPackages.Add(packageName, pkg);

#if IS_EDITOR
			if (Program.Game.IsEditor)
				Program.Game.AssetMgrForm.FillPackageList();
#endif

			return pkg;
		}
        #endregion

        #region IsValidPackageName
        /// <summary>
        /// Checks if the given package name is valid (i.e. is a valid
        /// filename)
        /// </summary>
        public static bool IsValidPackageName(string pkgName, out char invalidChar)
		{
			char[] chrs = Path.GetInvalidFileNameChars();
			invalidChar = ' ';

			for (int i = 0; i < pkgName.Length; i++)
			{
                if (CharArrayContains(chrs, pkgName[i]))
				{
					invalidChar = pkgName[i];
					return false;
				}
				if (pkgName[i] == '.')
				{
					invalidChar = '.';
					return false;
				}
			}
			
			return true;
		}
        #endregion
        		
        #region CharArrayContains
		/// <summary>
		/// Char array contains
		/// </summary>
		private static bool CharArrayContains(char[] chars, char findChar)
		{
			for (int i = 0; i < chars.Length; i++)
            {
            	if (chars[i] == findChar)
            		return true;
            }
		
			return false;
		} // CharArrayContains(chars, findChar)
        #endregion
        		
        #region ResaveAllPackages
		/// <summary>
		/// Loads all packages and then resaves them
		/// </summary>
		public void ResaveAllPackages(AssetSettings assetSettings)
		{
			foreach (KeyValuePair<string, string> pair in PackageMap)
			{
				Package pkg = LoadPackage(pair.Key, true, assetSettings);
				pkg.SetNeedsSave(true);
				pkg.Save(assetSettings);
			}
			
			// TODO: MessageBox.Show("All packages resaved.");
		}
        #endregion
        		
        #region AnyUnsavedPackages
		/// <summary>
		/// Returns true if there are any fully loaded unsaved packages.
		/// Also return a list of unsaved packages in the out param.
		/// </summary>
		public bool AnyUnsavedPackages(out List<Package> unsavedPackages)
		{
			bool result = false;
		
			unsavedPackages = new List<Package>();
		
			foreach (KeyValuePair<string, Package> pair in LoadedPackages)
			{
				if (pair.Value.NeedsSave)
				{
					result = true;
					unsavedPackages.Add(pair.Value);
				}
			}
			
			return result;
		}
        #endregion

        #region GetAllPackageNames
        /// <summary>
        /// Simply returns a list of all known packages
        /// </summary>
        /// <returns></returns>
        internal string[] GetAllPackageNames()
		{
			List<string> result = new List<string>();

			foreach (KeyValuePair<string, Package> pair in LoadedPackages)
			{
				result.Add(pair.Key.Substring(0, pair.Key.IndexOf('.')));
			}

			return result.ToArray();
		}
        #endregion
	}
}
