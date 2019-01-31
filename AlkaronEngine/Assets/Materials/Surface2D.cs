using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace AlkaronEngine.Assets.Materials
{
	/// <summary>
	/// Surface 2D
	/// </summary>
	public class Surface2D : Asset
	{
        /// <summary>
        /// Texture
        /// </summary>
        /// <returns>Texture 2D</returns>
        public Texture2D Texture { get; private set; } // Texture

        /// <summary>
        /// Is valid surface?
        /// </summary>
        public override bool IsValid
        {
            get
            {
                return Texture != null;
            } // get
        } // IsValid

        public const int MaxAssetVersion = 2;

        /// <summary>
        /// Texture filter minimum
        /// </summary>
        /// <returns>Texture filter</returns>
        public TextureFilter TextureFilterMin { get; set; } // TextureFilterMin
		/// <summary>
		/// Texture filter mag
		/// </summary>
		/// <returns>Texture filter</returns>
		public TextureFilter TextureFilterMag { get; set; } // TextureFilterMag
		/// <summary>
		/// Texture filter mip
		/// </summary>
		/// <returns>Texture filter</returns>
		public TextureFilter TextureFilterMip { get; set; } // TextureFilterMip

        /// <summary>
        /// Mip start
        /// </summary>
        private int[] mipStart;
        /// <summary>
        /// Mip size
        /// </summary>
        private int[] mipSize;
		/// <summary>
		/// Raw data
		/// </summary>
		private byte[] rawData;

		/// <summary>
		/// Create surface 2D
		/// </summary>
		public Surface2D()
		{
            Texture = null;
		} // Surface2D()

		/// <summary>
		/// Dispose
		/// </summary>
		public override void Dispose()
		{
			if (Texture != null)
            {
                Texture.Dispose();
            }
        } // Dispose()

		#region Load
		/// <summary>
		/// Loads a binary surface 2D
		/// </summary>
		public override void Load(string packageName, string assetName,
			Stream stream)
		{
			Name = assetName;
			PackageName = Path.GetFileNameWithoutExtension(packageName);

			BinaryReader reader = new BinaryReader(stream);
			{
				string magic = reader.ReadString();
				AssetVersion = reader.ReadInt32();

				OriginalFilename = reader.ReadString();

				int Width = reader.ReadInt32();
				int Height = reader.ReadInt32();
                int MipLevels = reader.ReadInt32();
				SurfaceFormat format = (SurfaceFormat)reader.ReadInt32();

				if (AssetVersion > 1)
				{
					TextureFilterMin = (TextureFilter)reader.ReadInt32();
					TextureFilterMag = (TextureFilter)reader.ReadInt32();
					TextureFilterMip = (TextureFilter)reader.ReadInt32();
				} // if (assetVersion)

                bool autoGenerateMipmaps = (MipLevels < 2);

				int count = reader.ReadInt32();
				rawData = reader.ReadBytes(count);

				try
				{
                    Texture2D newDXTexture = new Texture2D(AlkaronCoreGame.Core.GraphicsDevice,
                        Width, Height, autoGenerateMipmaps, format);
					if (newDXTexture != null)
					{
						// Dispose old texture
						if (Texture != null)
						{
                            Texture.Dispose();
                            Texture = null;
						} // if (dxTexture)

                        Texture = newDXTexture;
					} // if (newDXTexture)
				} // try
				catch (Exception ex)
				{
					AlkaronCoreGame.Core.Log("Failed to load Surface2D:\r\n" + ex);
					return;
				} // catch

				mipStart = new int[MipLevels];
				mipSize = new int[MipLevels];
				for (int i = 0; i < MipLevels; i++)
				{
					mipStart[i] = reader.ReadInt32();
					mipSize[i] = reader.ReadInt32();

                    Texture.SetData(i, null, rawData, mipStart[i], mipSize[i]);
				} // for (int)
			} // block
		} // Load(packageName, assetName, stream)
		#endregion

		#region Save
		/// <summary>
		/// 
		/// </summary>
		public override void Save(BinaryWriter writer)
		{
			writer.Write("HAF ");
			writer.Write(MaxAssetVersion);
			writer.Write(OriginalFilename);

			writer.Write(Texture.Width);
			writer.Write(Texture.Height);
			writer.Write(Texture.LevelCount);
			writer.Write((int)Texture.Format);

			writer.Write((int)TextureFilterMin);
			writer.Write((int)TextureFilterMag);
			writer.Write((int)TextureFilterMip);

			writer.Write(rawData.Length);
			writer.Write(rawData);

			for (int i = 0; i < Texture.LevelCount; i++)
			{
				writer.Write(mipStart[i]);
				writer.Write(mipSize[i]);
			} // for (int)
        } // Save(writer)
        #endregion
	} // class Surface2D
} // namespace AlkaronEngine.Assets.Materials
