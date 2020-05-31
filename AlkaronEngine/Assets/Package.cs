using AlkaronEngine.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Veldrid;

namespace AlkaronEngine.Assets
{
	public delegate void ChangedDelegate(Package package, bool NeedsSave);

    /// <summary>
    /// A package containing assets.
    /// 
    /// Can be a real package stored persistently on disk or a volatile package
    /// that cannot be saved an only exists at runtime (i.e. created through
    /// dynamic logic)
    /// 
    /// How assets are managed:
    /// The "LoadedAssets" dictionary contains all assets that are currently
    /// loaded into memory. This is different (any may be even distinct) from 
    /// what is stored in the "AssetOffsetMap" dictionary which defines where
    /// a specific asset (identified by name) is stored on disk inside the 
    /// package file.
    /// 
    /// Example:
    /// The "AssetOffsetMap" contains a few "Surface2D" assets which are stored
    /// in the actual package file on disk. When the package is initially loaded, the
    /// "AssetOffsetMap" is filled, but the "LoadedAssets" dictionary is empty,
    /// because none of the Surface2Ds were so far needed by other parts of the 
    /// program.
    /// Then a new "Material" asset is created at runtime and added to this package.
    /// This Material will be part of the "LoadedAssets" dictionary (because it
    /// was created at runtime and exists in memory), but it's not part of the
    /// AssetOffsetMap, because the package has not been written to disk yet.
    /// If one of the Surface2Ds in the "AssetOffsetMap" is needed, it will be loaded
    /// from disk and added to the "LoadedAssets" dictionary as well. It is then
    /// present in both dictionaries.
    /// </summary>
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
        private bool isVolatile;
		
		public const int MaxPackageVersion = 1;        

        public string PackageName
		{
			get
			{
				if (isVolatile)
                {
                    return PackageManager.VolatilePackageName;
                }

                return Path.GetFileName(fullFilename);
			}
		}

		public string PackageShortName
		{
			get
			{
				if (isVolatile)
                {
                    return PackageManager.VolatilePackageName;
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
		private bool IsFullyLoaded;

        /// <summary>
        /// Is Package currently loading?
        /// </summary>
        private bool IsLoading;

		/// <summary>
		/// Flag saying if the package needs a save
		/// </summary>
		public bool NeedsSave { get; private set; }
		
		public int PackageVersion { get; private set; }

        /// <summary>
        /// Indicates whether this package is readonly, i.e. it cannot be modified.
        /// 
        /// Packages loaded from an embedded resource have this flag set to
        /// true.
        /// </summary>
        public bool IsReadOnly { get; private set; }
		#endregion

		#region Package
		private Package(bool setIsReadOnly)
		{
            IsReadOnly = setIsReadOnly;

            AssetOffsetMap = new Dictionary<string, int>();
			LoadedAssets = new Dictionary<string, Asset>();

			PackageVersion = MaxPackageVersion;

			IsLoading = false;
            IsFullyLoaded = true;

            isVolatile = false;

            SetNeedsSave(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AlkaronEngine.Assets.Package"/> class.
        /// 
        /// Sets the filename as the origin and loads the index of the contained
        /// assets, but does not load the package completely.
        /// </summary>
        public Package(string filename, AssetSettings assetSettings, bool isReadOnly = false)
            : this(isReadOnly)
		{
			fullFilename = filename;

            if (File.Exists(fullFilename))
            {
                // Set FullyLoaded as false, when setting a filename
                IsFullyLoaded = false;

                Open(File.OpenRead(fullFilename));
            }
            else
            {
                // If this is a fresh package, mark it as fully loaded
                IsFullyLoaded = true;
            }
        }

        internal Package(Stream str, AssetSettings assetSettings, bool isReadOnly = false)
            : this(isReadOnly)
        {
            fullFilename = null;
            IsFullyLoaded = false;
            Open(str);
        }
		#endregion
		
		#region CreateVolatile
		/// <summary>
		/// Creates a runtime-only volatile package.
		/// </summary>
		internal static Package CreateVolatile()
		{
			Package pkg = new Package(false);
			pkg.fullFilename = "";
			pkg.isVolatile = true;
			pkg.IsFullyLoaded = true;
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
                pair.Value.SetPackageOwner(null);
				pair.Value.Dispose();
			}
			LoadedAssets.Clear();
		}
        #endregion

        #region Open
        /// <summary>
        /// Opens the package, reading the Asset index, but does not load
        /// any assets.
        /// </summary>
        public void Open(Stream stream)
        {
            AssetOffsetMap.Clear();

            using (BinaryReader reader = new BinaryReader(stream))
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
            }
        }
        #endregion

        #region Load
        /// <summary>
        /// Loads all assets in the package
        /// </summary>
        public void LoadAllAssets(AssetSettings assetSettings, Stream stream = null)
		{
			if (IsFullyLoaded == true)
            {
                return;
            }

			try
			{
                if (stream == null)
                {
                    if (File.Exists(fullFilename) == false)
                    {
                        // TODO: Return error information
                        return;
                    }

                    stream = File.OpenRead(fullFilename);
                }

                IsLoading = true;

                AssetOffsetMap.Clear();

                using (BinaryReader reader = new BinaryReader(stream))
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
                        if (LoadAssetInternal(reader, assetSettings) == false)
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

                IsFullyLoaded = true;
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
		private bool LoadAssetInternal(BinaryReader reader, AssetSettings assetSettings)
		{
			string assetName = "<Unknown>";

			try
			{
				string assetTypeName = reader.ReadString();

				Type assetType = Type.GetType(assetTypeName);
				if (assetType == null)
				{
                    Log.Status("Found possibly deprecated or unknown asset type: " +
						assetTypeName);
					return false;
				}

				Asset newAsset = assetType.InvokeMember(null,
					System.Reflection.BindingFlags.CreateInstance, null,
					null, new object[] { }) as Asset;
				newAsset.Deserialize(reader, assetSettings);
                newAsset.SetPackageOwner(this);

                assetName = newAsset.Name;

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
                Log.Status("Error while loading asset '" + assetName + "':\r\n" + ex);
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
		private bool LoadAsset(string assetName, AssetSettings assetSettings)
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

            if (File.Exists(fullFilename) == false)
            {
                return false;
            }

            long offset = AssetOffsetMap[assetName];

			using (BinaryReader reader = new BinaryReader(File.OpenRead(fullFilename)))
			{
				reader.BaseStream.Seek(offset, SeekOrigin.Begin);

				if (LoadAssetInternal(reader, assetSettings) == false)
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
        public Asset GetAsset(string assetName, AssetSettings assetSettings)
		{
            // If we have alread loaded the asset previously, then just return it.
            if (LoadedAssets.ContainsKey(assetName) == true)
            {
                return LoadedAssets[assetName];
            }

            bool result = LoadAsset(assetName, assetSettings);
            if (result == false)
            {
                // Couldn't load the asset from disk.
                return null; 
            }

            return LoadedAssets[assetName];
		}
        #endregion

        #region Save
		/// <summary>
		/// Saves the package
		/// </summary>
		public bool Save(AssetSettings assetSettings)
		{
			if (isVolatile || IsReadOnly)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(fullFilename))
            {
                return false;
            }

            if (IsFullyLoaded == false)
			{
                // Before saving, we need to fully load the package, to 
                // correctly save all assets

				LoadAllAssets(assetSettings);

                // If IsLoaded is still false after trying to load the package,
                // then something went wrong and we can't continue
                if (IsFullyLoaded == false)
                {
                    Log.Status("Saving package failed");
                    return false;
                }
            }

            string directory = Path.GetDirectoryName(fullFilename);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = AlkaronCoreGame.Core.ContentDirectory;
            }
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

                    string assetType = pair.Value.GetType().ToString();
                    writer.Write(assetType);
					pair.Value.Serialize(writer, assetSettings);
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
		public void StoreAsset(Asset asset)
		{
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset)); 
            }

            if (IsReadOnly)
            {
                return; 
            }

            string assetName = asset.Name;

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

            asset.SetPackageOwner(this);

			SetNeedsSave(true);
		}
        #endregion

        #region StoreAsset
		/// <summary>
		/// Removes an asset from the package. This requires loading all
        /// assets in the package.
		/// </summary>
		internal void DeleteAsset(Asset SelectedAsset, AssetSettings assetSettings)
		{
            if (IsReadOnly)
            {
                return; 
            }

			if (IsFullyLoaded == false)
			{
				LoadAllAssets(assetSettings);

                // If IsLoaded is still false after trying to load the package,
                // then something went wrong and we can't continue
                if (IsFullyLoaded == false)
                {
                    Log.Status("Delete package failed");
                    return;
                }
            }

			if (LoadedAssets.ContainsKey(SelectedAsset.Name) == false)
            {
                return;
            }

            LoadedAssets.Remove(SelectedAsset.Name);
            SelectedAsset.SetPackageOwner(null);

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
                pair.Value.SetPackageOwner(null);
				pair.Value.Dispose();
			}
			LoadedAssets.Clear();

			using (BinaryWriter writer = new BinaryWriter(File.Create(fullFilename)))
			{
                // Base header
                writer.Write("HPKG");
                writer.Write(MaxPackageVersion);
                // Empty asset list
                writer.Write(LoadedAssets.Count);

                long offsetMapPos = writer.BaseStream.Position;
                writer.Write((int)0);       // Offset of the AssetOffsetMap
            }

            IsFullyLoaded = true;

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
			if (isVolatile)
            {
                return;
            }

            if (IsReadOnly)
            {
                NeedsSave = false;
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

            return (from a in LoadedAssets.Values
                    where a.AssetType.ToLowerInvariant() == type
                    select a).ToArray();
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
