using System;
using System.IO;

namespace AlkaronEngine.Assets
{
	public abstract class Asset : IAsset, IDisposable
	{
		/// <summary>
		/// Name of this asset
		/// </summary>
		public string Name { get; set; } // Name
		
		/// <summary>
		/// Package to which this asset belongs to
		/// </summary>
		public string PackageName { get; set; } // PackageName

        /// <summary>
        /// Package to which this asset belongs to
        /// </summary>
        public Package Package
		{
			get
			{
				return AlkaronCoreGame.Core.PackageManager.LoadPackage(PackageName);
			}
		}

		/// <summary>
		/// The original filename from which this asset was imported.
		/// </summary>
		public string OriginalFilename;

		/// <summary>
		/// Loads the asset from the specified file
		/// </summary>
		public abstract void Load(string packageName, string assetName,
			Stream stream);

		/// <summary>
		/// Saves the asset in the binary form into the stream writer.
		/// </summary>
		public abstract void Save(BinaryWriter writer);

		/// <summary>
		/// Version of this asset
		/// </summary>
		public int AssetVersion { get; protected set; }

		/// <summary>
		/// Returns true if this Asset is valid and usable
		/// </summary>
		public abstract bool IsValid { get; }
		
		/// <summary>
		/// Returns the fully qualified name of this asset, which
		/// can be used to reference this asset in actors.
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
		/// Dispose resources used by this asset
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Can this asset be reimported from disk?
		/// (e.g. NodeMaterials can't)
		/// </summary>
		public virtual bool SupportsReimportFromFile
		{
			get
			{
				return true;
			}
		}
	}
}
