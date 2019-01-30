using System;
using System.Collections.Generic;
using System.IO;

namespace AlkaronEngine.Assets
{
	public delegate void ChangedDelegate(Package package, bool NeedsSave);

	public class Package : IDisposable
	{
		#region Members
		/// <summary>
		/// Filename of this package on disk
		/// </summary>
		private string fullFilename;

        /// <summary>
        /// Means this package cannot be saved on disk and is
        /// only used at runtime.
        /// </summary>
        private bool isTransient;
		
		public const int MaxPackageVersion = 1;

		public string PackageName
		{
			get
			{
				if (isTransient)
                {
                    return "Transient";
                }

                return Path.GetFileName(fullFilename);
			}
		}

		public string PackageShortName
		{
			get
			{
				if (isTransient)
                {
                    return "Transient";
                }

                return Path.GetFileNameWithoutExtension(fullFilename);
			}
		}

        private Dictionary<string, int> AssetOffsetMap;
        private Dictionary<string, Asset> LoadedAssets;
		
		public event ChangedDelegate OnPackageChanged;

        /// <summary>
        /// Is Package loaded?
        /// </summary>
		private bool IsLoaded;

        /// <summary>
        /// Is Package currently loading?
        /// </summary>
        private bool IsLoading;

		/// <summary>
		/// Flag saying if the package needs a save
		/// </summary>
		public bool NeedsSave { get; private set; }
		
		public int PackageVersion { get; private set; }
		#endregion

		#region Package
		private Package()
		{
			AssetOffsetMap = new Dictionary<string, int>();
			LoadedAssets = new Dictionary<string, Asset>();

			PackageVersion = MaxPackageVersion;

			IsLoading = false;
			IsLoaded = false;
		}
		
		public Package(string filename)
		{
			fullFilename = filename;

			AssetOffsetMap = new Dictionary<string, int>();
			LoadedAssets = new Dictionary<string, Asset>();
			
			PackageVersion = MaxPackageVersion;

			IsLoading = false;
			IsLoaded = false;
			
			isTransient = false;

			SetNeedsSave(false);
		}
		#endregion
		
		#region CreateTransient
		/// <summary>
		/// Creates a runtime-only transient package.
		/// </summary>
		internal static Package CreateTransient()
		{
			Package pkg = new Package();
			pkg.fullFilename = "";
			pkg.isTransient = true;
			pkg.IsLoaded = true;
			pkg.NeedsSave = false;
			return pkg;
		}
		#endregion
		
		#region Dispose
		public void Dispose()
		{
			// Clear asset list
			foreach (KeyValuePair<string, Asset> pair in LoadedAssets)
			{
				pair.Value.Dispose();
			}
			LoadedAssets.Clear();
		}
		#endregion

		#region Load
		/// <summary>
		/// Loads all assets in the package
		/// </summary>
		public void Load()
		{
			if (File.Exists(fullFilename) == false ||
				IsLoaded == true)
            {
                return;
            }

            IsLoading = true;

			try
			{
				AssetOffsetMap.Clear();

				using (BinaryReader reader =
					new BinaryReader(File.OpenRead(fullFilename)))
				{
					string magic = reader.ReadString();
					PackageVersion = reader.ReadInt32();

					int numberOfAssets = reader.ReadInt32();
					int offsetOffsetMap = reader.ReadInt32();

					// We have to store the offsets seperately for the loading process
					// to be able to skip assets that fail to load
					int[] offsets = new int[numberOfAssets];

					long curPos = reader.BaseStream.Position;
					reader.BaseStream.Seek(offsetOffsetMap, SeekOrigin.Begin);

					for (int i = 0; i < numberOfAssets; i++)
					{
						string assetName = reader.ReadString();
						int offset = reader.ReadInt32();

						offsets[i] = offset;

						AssetOffsetMap.Add(assetName, offset);
					}

					reader.BaseStream.Seek(curPos, SeekOrigin.Begin);
					for (int i = 0; i < numberOfAssets; i++)
					{
						if (LoadAssetInternal(reader) == false)
						{
							// Asset failed to load. We have to skip it.

							// If this is the last one, just abort loading.
							if (i == numberOfAssets - 1)
                            {
                                break;
                            }

                            // Else we have to seek to the next asset
                            reader.BaseStream.Seek(offsets[i + 1], SeekOrigin.Begin);
						}
					}
				}

				IsLoaded = true;
			}
			finally
			{
				IsLoading = false;
			}
		}

		#region LoadAssetInternal
		/// <summary>
		/// The actual loading function for an asset. Returns false if loading
		/// failed for some reason.
		/// </summary>
		private bool LoadAssetInternal(BinaryReader reader)
		{
			string assetName = "<Unknown>";

			try
			{
				assetName = reader.ReadString();
				string assetTypeName = reader.ReadString();

				Type assetType = Type.GetType(assetTypeName);
				if (assetType == null)
				{
					AlkaronCoreGame.Core.Log("Found possibly deprecated or unknown asset type: " +
						assetTypeName);
					return false;
				}

				Asset newAsset = assetType.InvokeMember(null,
					System.Reflection.BindingFlags.CreateInstance, null,
					null, new object[] { }) as Asset;
				newAsset.Load(PackageName, assetName, reader.BaseStream);

				if (LoadedAssets.ContainsKey(assetName))
                {
                    LoadedAssets[assetName] = newAsset;
                }
                else
                {
                    LoadedAssets.Add(assetName, newAsset);
                }

                return true;
			}
			catch (Exception ex)
			{
                AlkaronCoreGame.Core.Log("Error while loading asset '" + assetName + "':\r\n" + ex);
				return false;
			}
		}
		#endregion

		#region LoadAsset
		/// <summary>
		/// Loads a specfic asset from the package using the AssetOffsetMap.
		/// Returns false if loading failed.
		/// 
		/// Does nothing (and returns true) if the asset was already loaded before.
		/// </summary>
		private bool LoadAsset(string assetName)
		{
			// Check if we have already loaded the asset
			if (LoadedAssets.ContainsKey(assetName))
            {
                return true;
            }

            if (AssetOffsetMap.ContainsKey(assetName) == false)
            {
                return false;
            }

            long offset = AssetOffsetMap[assetName];

			using (BinaryReader reader =
					new BinaryReader(File.OpenRead(fullFilename)))
			{
				reader.BaseStream.Seek(offset, SeekOrigin.Begin);

				if (LoadAssetInternal(reader) == false)
                {
                    return false;
                }
            }

			return true;
		}
		#endregion
		#endregion

		#region GetAsset
		/// <summary>
		/// Returns the loaded asset
		/// </summary>
		public Asset GetAsset(string assetName)
		{
			if (IsLoaded == false)
			{
				// If we have loaded the package partially already,
				// we may habe loaded the asset, so return it in that case.
				if (LoadedAssets.ContainsKey(assetName) == true)
                {
                    return LoadedAssets[assetName];
                }

                if (IsLoading)
				{
					if (LoadAsset(assetName) == false)
					{
						// We're currently in the process of loading this package
						// and need an asset which hasn't been loaded yet.
						// This is a fatal error and we cannot continue.
						return null;
					}

					// Return the loaded asset
					return LoadedAssets[assetName];
				}

				Load();

				if (IsLoaded == false)
                {
                    return null;
                }
            }

			if (LoadedAssets.ContainsKey(assetName) == false)
            {
                return null;
            }

            return LoadedAssets[assetName];
		}
        #endregion

        #region Save
		/// <summary>
		/// Saves the package
		/// </summary>
		public bool Save()
		{
			if (isTransient)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(fullFilename))
            {
                return false;
            }

            if (IsLoaded == false)
			{
				Load();

                // If IsLoaded is still false after trying to load the package,
                // then something went wrong and we can't continue
                if (IsLoaded == false)
                {
                    AlkaronCoreGame.Core.Log("Saving package failed");
                    return false;
                }
            }

			string directory = Path.GetDirectoryName(fullFilename);
			string tempName = Path.Combine(directory, "tempPackage.tmp");
			if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            using (BinaryWriter writer = new BinaryWriter(File.Create(tempName)))
			{
				writer.Write("HPKG");
				writer.Write(MaxPackageVersion);
				writer.Write(LoadedAssets.Count);
				
				long offsetMapPos = writer.BaseStream.Position;
				writer.Write((int)0);		// Offset of the AssetOffsetMap

				AssetOffsetMap.Clear();
				foreach (KeyValuePair<string, Asset> pair in LoadedAssets)
				{
					AssetOffsetMap.Add(pair.Key, (int)writer.BaseStream.Position);

					writer.Write(pair.Key);
					writer.Write(pair.Value.GetType().ToString());
					pair.Value.Save(writer);
				}

				long curPos = writer.BaseStream.Position;
				writer.BaseStream.Seek(offsetMapPos, SeekOrigin.Begin);
				writer.Write((int)curPos);	// Write offset of the AssetOffsetMap
				writer.BaseStream.Seek(curPos, SeekOrigin.Begin);
				foreach (KeyValuePair<string, int> pair in AssetOffsetMap)
				{
					writer.Write(pair.Key);
					writer.Write(pair.Value);
				}
			}

			// Delete original file
			File.Delete(fullFilename);
			// Rename new file to original file
			File.Move(tempName, fullFilename);

			SetNeedsSave(false);

            return true;
		}
        #endregion

        #region StoreAsset
		/// <summary>
		/// Stores the asset in the package
		/// </summary>
		public void StoreAsset(string assetName, Asset asset)
		{
			if (LoadedAssets.ContainsKey(assetName))
			{
				if (LoadedAssets[assetName] != asset)
				{
					LoadedAssets[assetName].Dispose();
					LoadedAssets[assetName] = asset;
				}
			}
			else
			{
				LoadedAssets.Add(assetName, asset);
			}

			SetNeedsSave(true);
		}
        #endregion

        #region StoreAsset
		/// <summary>
		/// Removes an asset from the package
		/// </summary>
		internal void DeleteAsset(Asset SelectedAsset)
		{
			if (IsLoaded == false)
			{
				Load();

                // If IsLoaded is still false after trying to load the package,
                // then something went wrong and we can't continue
                if (IsLoaded == false)
                {
                    AlkaronCoreGame.Core.Log("Delete package failed");
                    return;
                }
            }

			if (LoadedAssets.ContainsKey(SelectedAsset.Name) == false)
            {
                return;
            }

            LoadedAssets.Remove(SelectedAsset.Name);

			SetNeedsSave(true);
		}
        #endregion

        #region Create
		/// <summary>
		/// Creates an empty package
		/// </summary>
		public void Create()
		{
			// Clear asset list
			foreach (KeyValuePair<string, Asset> pair in LoadedAssets)
			{
				pair.Value.Dispose();
			}
			LoadedAssets.Clear();

			using (BinaryWriter writer = new BinaryWriter(File.Create(fullFilename)))
			{
				// Write empty number of assets
				writer.Write(LoadedAssets.Count);
			}

			IsLoaded = true;

			SetNeedsSave(true);
		}
        #endregion

        #region SetNeedsSave
		/// <summary>
		/// Sets the correct NeedsSave flag and updates the TreeNode in
		/// the AssetManager.
		/// </summary>
		public void SetNeedsSave(bool setNeedsSave)
		{
			if (isTransient)
            {
                return;
            }

            NeedsSave = setNeedsSave;

            OnPackageChanged?.Invoke(this, NeedsSave);

#if IS_EDITOR
            if (Program.Game.IsEditor)
			{
				System.Windows.Forms.TreeNode node =
					Program.Game.AssetMgrForm.FindPackageNode(PackageName);

				if (node != null)
				{
					if (NeedsSave)
					{
						if (node.Text.StartsWith("*"))
							return;

						node.Text = "*" + node.Text;
					}
					else
					{
						if (node.Text.StartsWith("*") == false)
							return;

						node.Text = node.Text.Substring(1);
					}

					// Let the editor refresh the package contents
					Program.Game.AssetMgrForm.ShowPackageContents(this);
				}
			}
#endif
        }
        #endregion

        #region GetAssetsByType
		/// <summary>
		/// Returns an array of all assets in this package of the specified
		/// type (e.g. "Surface2D").
		/// "type" is NOT case-sensitive.
		/// </summary>
		public Asset[] GetAssetsByType(string type)
		{
			type = type.ToLowerInvariant();

			List<Asset> resultList = new List<Asset>();
			foreach (KeyValuePair<string, Asset> pair in LoadedAssets)
			{
				if (pair.Value.AssetType.ToLowerInvariant() == type)
					resultList.Add(pair.Value);
			}
			return resultList.ToArray();
		}
        #endregion

#if IS_EDITOR
        #region PopulateListView
		public void PopulateListView(System.Windows.Forms.ListView list)
		{
			foreach (KeyValuePair<string, Asset> pair in LoadedAssets)
			{
				System.Windows.Forms.ListViewItem lvi = 
					new System.Windows.Forms.ListViewItem(pair.Key);
				lvi.Tag = pair.Value;
				list.Items.Add(lvi);
			}
		}
        #endregion
#endif
    }
}
