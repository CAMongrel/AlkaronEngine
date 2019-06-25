using System;
using System.IO;
using Veldrid;

namespace AlkaronEngine.Assets.Materials
{
	/// <summary>
	/// Surface 2D
	/// </summary>
	public class Surface2D : Asset
	{
        public Texture Texture { get; private set; }

        public override bool IsValid
        {
            get
            {
                return Texture != null;
            } // get
        }

        protected override int MaxAssetVersion => 1;

        public SamplerFilter SamplerFilter { get; set; }

		public Surface2D()
		{
            Texture = null;
		}

        public Surface2D(Texture setTexture)
        {
            Texture = setTexture;
        }

        public override void Dispose()
		{
			if (Texture != null)
            {
                Texture.Dispose();
            }
        } 

        #region Deserialize
        public override void Deserialize(BinaryReader reader, AssetSettings assetSettings)
		{
            base.Deserialize(reader, assetSettings);

            uint width = reader.ReadUInt32();
            uint height = reader.ReadUInt32();
            uint mipLevels = reader.ReadUInt32();
            PixelFormat format = (PixelFormat)reader.ReadByte();
            TextureSampleCount sampleCount = (TextureSampleCount)reader.ReadByte();

            bool autoGenerateMipmaps = (mipLevels < 2);

            int count = reader.ReadInt32();
            ReadOnlySpan<byte> rawData = reader.ReadBytes(count);

            try
            {
                TextureDescription textureDescription = new TextureDescription()
                {
                    Width = width,
                    Height = height,
                    MipLevels = mipLevels,
                    Depth = 1,
                    Format = format,
                    SampleCount = sampleCount,
                    Type = TextureType.Texture2D,
                    Usage = assetSettings.ReadOnlyAssets ? TextureUsage.Sampled : TextureUsage.Staging
                };

                Texture newDXTexture = assetSettings.GraphicsDevice.ResourceFactory.CreateTexture(textureDescription);
                if (newDXTexture != null)
                {
                    // Dispose old texture
                    Texture?.Dispose();

                    Texture = newDXTexture;
                }
            }
            catch (Exception ex)
            {
                AlkaronCoreGame.Core.Log("Failed to load Surface2D:\r\n" + ex);
                return;
            }

            uint[] mipStart = new uint[mipLevels];
            uint[] mipSize = new uint[mipLevels];
            uint[] mipWidth = new uint[mipLevels];
            uint[] mipHeight = new uint[mipLevels];
            for (uint i = 0; i < mipLevels; i++)
            {
                mipStart[i] = reader.ReadUInt32();
                mipSize[i] = reader.ReadUInt32();
                mipWidth[i] = reader.ReadUInt32();
                mipHeight[i] = reader.ReadUInt32();

                ReadOnlySpan<byte> mipSlice = rawData.Slice((int)mipStart[i], (int)mipSize[i]);
                assetSettings.GraphicsDevice.UpdateTexture<byte>(Texture, mipSlice.ToArray(), 0, 0, 0, mipWidth[i], mipHeight[i], 1, i, 0);
            }
        }
        #endregion

        #region Serialize
        /// <summary>
        /// 
        /// </summary>
        public override void Serialize(BinaryWriter writer, AssetSettings assetSettings)
		{
            if (Texture.Usage.HasFlag(TextureUsage.Staging) == false)
            {
                throw new InvalidOperationException("Cannot serialize readonly texture");
            }

            base.Serialize(writer, assetSettings);

            /*writer.Write(Texture.Width);
			writer.Write(Texture.Height);
            writer.Write(Texture.MipLevels);
            writer.Write((byte)Texture.Format);
            writer.Write((byte)Texture.SampleCount);

            uint[] mipStart = new uint[Texture.MipLevels];
            uint[] mipSize = new uint[Texture.MipLevels];

            uint texelCount = 0;
            uint mipWidth = Texture.Width;
            uint mipHeight = Texture.Height;
            for (int i = 0; i < Texture.MipLevels; i++)
            {
                Texture

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
            */
        }
        #endregion
    }
}
