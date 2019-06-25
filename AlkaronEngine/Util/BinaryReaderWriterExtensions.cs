using System;
using System.IO;
using System.Numerics;

namespace AlkaronEngine.Util
{
    public static class BinaryReaderWriterExtensions
    {
        public static void Write(this BinaryWriter writer, Vector4 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
            writer.Write(vector.W);
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()); 
        }

        public static void Write(this BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        /*public static void Write(this BinaryWriter writer, Asset asset)
        {
            if (asset == null ||
                string.IsNullOrWhiteSpace(asset.Fullname))
            {
                writer.Write(""); 
            }
            else
            {
                writer.Write(asset.Fullname);
            }
        }

        public static T ReadAsset<T>(this BinaryReader reader) where T : Asset, new()
        {
            string asset_ref = reader.ReadString();
            if (string.IsNullOrWhiteSpace(asset_ref))
            {
                return default(T); 
            }
            else
            {
                return AlkaronCoreGame.Core.AssetManager.Load<T>(asset_ref); 
            }
        }*/
    }
}
