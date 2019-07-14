using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Veldrid;

namespace AlkaronEngine.Assets.Materials
{
    /// <summary>
    /// Surface 2D
    /// </summary>
    public class Surface2D : Asset
    {
        public Texture Texture { get; private set; }

        public TextureView View { get; private set; }

        public override bool IsValid
        {
            get
            {
                return Texture != null;
            } // get
        }

        protected override int MaxAssetVersion => 1;

        public SamplerFilter SamplerFilter { get; set; }

        public int Width => IsValid ? (int)Texture.Width : 0;
        public int Height => IsValid ? (int)Texture.Height : 0;

        public Surface2D()
        {
            Texture = null;
        }

        internal Surface2D(Texture setTexture, AssetSettings assetSettings)
        {
            Texture = setTexture;
            if (Texture.Usage == TextureUsage.Sampled ||
                Texture.Usage == TextureUsage.Storage)
            {
                View = assetSettings.GraphicsDevice.ResourceFactory.CreateTextureView(Texture);
            }
        }

        public override void Dispose()
        {
            if (Texture != null)
            {
                Texture.Dispose();
            }
        }

        #region Deserialize
        internal override void Deserialize(BinaryReader reader, AssetSettings assetSettings)
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
                TextureDescription textureDescription = TextureDescription.Texture2D(width, height, mipLevels, 1, format,
                    assetSettings.ReadOnlyAssets ? TextureUsage.Sampled : TextureUsage.Staging);

                Texture newDXTexture = assetSettings.GraphicsDevice.ResourceFactory.CreateTexture(textureDescription);
                if (newDXTexture != null)
                {
                    // Dispose old texture
                    Texture?.Dispose();

                    Texture = newDXTexture;
                }

                if (Texture.Usage == TextureUsage.Sampled ||
                    Texture.Usage == TextureUsage.Storage)
                {
                    View = assetSettings.GraphicsDevice.ResourceFactory.CreateTextureView(Texture);
                }
            }
            catch (Exception ex)
            {
                AlkaronCoreGame.Core.Log("Failed to load Surface2D:\r\n" + ex);
                return;
            }

            int[] mipStart = new int[mipLevels];
            int[] mipSize = new int[mipLevels];
            uint mipWidth = width;
            uint mipHeight = height;
            for (uint i = 0; i < mipLevels; i++)
            {
                mipStart[i] = reader.ReadInt32();
                mipSize[i] = reader.ReadInt32();

                ReadOnlySpan<byte> mipSlice = rawData.Slice((int)mipStart[i], (int)mipSize[i]);
                assetSettings.GraphicsDevice.UpdateTexture<byte>(Texture, mipSlice.ToArray(), 0, 0, 0, mipWidth, mipHeight, 1, i, 0);

                mipWidth /= 2;
                mipHeight /= 2;
            }
        }
        #endregion

        #region Serialize
        /// <summary>
        /// 
        /// </summary>
        internal override void Serialize(BinaryWriter writer, AssetSettings assetSettings)
        {
            if (Texture.Usage.HasFlag(TextureUsage.Staging) == false)
            {
                throw new InvalidOperationException("Cannot serialize readonly texture");
            }

            base.Serialize(writer, assetSettings);

            writer.Write(Texture.Width);
			writer.Write(Texture.Height);
            writer.Write(Texture.MipLevels);
            writer.Write((byte)Texture.Format);
            writer.Write((byte)Texture.SampleCount);

            int[] mipStart = new int[Texture.MipLevels];
            int[] mipSize = new int[Texture.MipLevels];

            int texelCount = 0;
            int mipWidth = (int)Texture.Width;
            int mipHeight = (int)Texture.Height;
            for (int i = 0; i < Texture.MipLevels; i++)
            {
                mipStart[i] = texelCount;
                mipSize[i] = (int)((float)mipWidth * (float)mipHeight *
                    GetPixelStride(Texture.Format));

                texelCount += mipSize[i];

                mipWidth /= 2;
                mipHeight /= 2;

                if (mipWidth < 1)
                {
                    mipWidth = 1;
                }
                if (mipHeight < 1)
                {
                    mipHeight = 1;
                }
            }
            byte[] data = new byte[texelCount];

            for (int i = 0; i < Texture.MipLevels; i++)
            {
                var mapped = assetSettings.GraphicsDevice.Map(Texture, MapMode.Read, (uint)i);
                Marshal.Copy(mapped.Data, data, mipStart[i], mipSize[i]);
                assetSettings.GraphicsDevice.Unmap(Texture, (uint)i);
            }

            writer.Write(texelCount);
            writer.Write(data);

            for (int i = 0; i < Texture.MipLevels; i++)
            {
                writer.Write(mipStart[i]);
                writer.Write(mipSize[i]);
            }
        }

        private static int GetPixelStride(PixelFormat pixelFormat)
        {
            return 4;
        }
        #endregion
    }

    internal static class ImageSharpTextureExtension
    { 
        internal static unsafe Texture CreateTextureWithUsage(this Veldrid.ImageSharp.ImageSharpTexture texture, GraphicsDevice gd, ResourceFactory factory, 
            TextureUsage usage)
        {
            Texture tex = factory.CreateTexture(TextureDescription.Texture2D(
                texture.Width, texture.Height, texture.MipLevels, 1, texture.Format, usage));
            for (int level = 0; level < texture.MipLevels; level++)
            {
                Image<Rgba32> image = texture.Images[level];
                fixed (void* pin = &MemoryMarshal.GetReference(image.GetPixelSpan()))
                {
                    gd.UpdateTexture(
                        tex,
                        (IntPtr)pin,
                        (uint)(texture.PixelSizeInBytes * image.Width * image.Height),
                        0,
                        0,
                        0,
                        (uint)image.Width,
                        (uint)image.Height,
                        1,
                        (uint)level,
                        0);
                }
            }

            return tex;
        }
    }
}
