using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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

        protected override int MaxAssetVersion => 1;

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
		/// Create surface 2D
		/// </summary>
		public Surface2D()
		{
            Texture = null;
		} // Surface2D()

        public Surface2D(Texture2D setTexture)
        {
            Texture = setTexture;
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

        #region Deserialize
        /// <summary>
        /// Loads a binary surface 2D
        /// </summary>
        public override void Deserialize(BinaryReader reader)
		{
            base.Deserialize(reader);

            int Width = reader.ReadInt32();
            int Height = reader.ReadInt32();
            int MipLevels = reader.ReadInt32();
            SurfaceFormat format = (SurfaceFormat)reader.ReadInt32();

            bool autoGenerateMipmaps = (MipLevels < 2);

            int count = reader.ReadInt32();
            byte[] rawData = reader.ReadBytes(count);

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

            int[] mipStart = new int[MipLevels];
            int[] mipSize = new int[MipLevels];
            for (int i = 0; i < MipLevels; i++)
            {
                mipStart[i] = reader.ReadInt32();
                mipSize[i] = reader.ReadInt32();

                Texture.SetData(i, null, rawData, mipStart[i], mipSize[i]);
            } // for (int)
		} // Load(packageName, assetName, stream)
        #endregion

        #region Serialize
        /// <summary>
        /// 
        /// </summary>
        public override void Serialize(BinaryWriter writer)
		{
            base.Serialize(writer);

            writer.Write(Texture.Width);
			writer.Write(Texture.Height);
			writer.Write(Texture.LevelCount);
			writer.Write((int)Texture.Format);

            int[] mipStart = new int[Texture.LevelCount];
            int[] mipSize = new int[Texture.LevelCount];

            int texelCount = 0;
            int mipWidth = Texture.Width;
            int mipHeight = Texture.Height;
            for (int i = 0; i < Texture.LevelCount; i++)
            {
                mipStart[i] = texelCount;
                mipSize[i] = (int)((float)mipWidth * (float)mipHeight *
                    GetPixelStride(Texture.Format));

                texelCount += mipSize[i];

                mipWidth /= 2;
                mipHeight /= 2;

                if (IsCompressedFormat(Texture.Format))
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

            for (int i = 0; i < Texture.LevelCount; i++)
            {
                Texture.GetData<byte>(i, null, data, mipStart[i], mipSize[i]);
            }

            writer.Write(texelCount);
            writer.Write(data);

            for (int i = 0; i < Texture.LevelCount; i++)
            {
                writer.Write(mipStart[i]);
                writer.Write(mipSize[i]);
            }
        } // Save(writer)
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
    } // class Surface2D
} // namespace HellspawnEngine.Assets.Materials
