using System;
using System.IO;
using Veldrid;

namespace AlkaronEngine.Assets
{
	public abstract class Asset : IDisposable
	{
		/// <summary>
		/// Name of this asset
		/// </summary>
		public string Name { get; set; } // Name

        /// <summary>
        /// Package to which this asset belongs to
        /// </summary>
        public Package Package { get; protected set; }

        /// <summary>
        /// Name of the package this asset belongs to
        /// </summary>
        public string PackageName
        {
            get
            {
                return Package?.PackageName ?? "<No Package>"; 
            } 
        }

		/// <summary>
		/// The original filename from which this asset was imported.
		/// </summary>
		public string OriginalFilename;

		/// <summary>
		/// Version of this asset
		/// </summary>
		public int AssetVersion { get; protected set; }

        /// <summary>
        /// Current maximum version (applied on every serialization)
        /// </summary>
        protected virtual int MaxAssetVersion => 1;

		/// <summary>
		/// Returns true if this Asset is valid and usable
		/// </summary>
		public abstract bool IsValid { get; }
		
		/// <summary>
		/// Returns the fully qualified name of this asset, which
		/// can be used to reference this asset in other assets or runtime
        /// objects (like actors).
		/// </summary>
		public string Fullname
		{
			get
			{
				return PackageName + "." + Name;
			}
		}

		/// <summary>
		/// Returns the name of the asset without the extension (e.g. 
		/// "DiffuseTexture.Surface2D" -> "DiffuseTexture"
		/// </summary>
		public string ShortName
		{
			get
			{
				return Path.GetFileNameWithoutExtension(Name);
			}
		}

		/// <summary>
		/// Returns the type of the asset (e.g. "Surface2D")
		/// </summary>
		public string AssetType
		{
			get
			{
				// Cut off the leading "."
				return Path.GetExtension(Name).Substring(1);
			}
		}

		/// <summary>
		/// Returns the name as the string of this object
		/// </summary>
		public override string ToString()
		{
			return this.GetType().Name + " " + Name;
		}

		/// <summary>
		/// Can this asset be reimported from disk?
		/// (e.g. NodeMaterials can't).
        /// 
        /// Base implementation returns true if "OriginalFilename" is set.
		/// </summary>
		public virtual bool SupportsReimportFromFile
		{
			get
			{
				return string.IsNullOrWhiteSpace(OriginalFilename) == false;
			}
		}

        /// <summary>
        /// Sets the package owner.
        /// </summary>
        public void SetPackageOwner(Package packetToAddTo)
        {
            Package = packetToAddTo;
        }

        /// <summary>
        /// Loads the asset from the specified binary reader
        /// </summary>
        public virtual void Deserialize(BinaryReader reader, AssetSettings assetSettings)
        {
            string magic = new string(reader.ReadChars(4));
            AssetVersion = reader.ReadInt32();
            Name = reader.ReadString();
            OriginalFilename = reader.ReadString();
        }

        /// <summary>
        /// Saves the asset in the binary form into the binary writer.
        /// </summary>
        public virtual void Serialize(BinaryWriter writer, AssetSettings assetSettings)
        {
            if (assetSettings.ReadOnlyAssets == true)
            {
                throw new InvalidOperationException("Cannot serialize assets in ReadOnlyAssets mode");
            }

            writer.Write("AEAF".ToCharArray());
            writer.Write(MaxAssetVersion);
            writer.Write(Name);
            writer.Write(OriginalFilename ?? string.Empty);
        }

        /// <summary>
        /// Dispose resources used by this asset
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
