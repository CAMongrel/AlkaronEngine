namespace AlkaronEngine.Assets
{
	public interface IAsset
	{
		/// <summary>
		/// Name
		/// </summary>
		string Name { get; set; } // Name

		/// <summary>
		/// Package to which this asset belongs to
		/// </summary>
		string PackageName { get; set; } // PackageName
	}
}
