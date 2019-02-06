using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Assets.Materials
{
    public class Material : Asset
    {
        private const int MaxAssetVersion = 1;

        internal enum GraphicsLibrary
        {
            DirectX,
            OpenGL 
        }

        internal enum CodeType
        {
            XNAEffect
        }

        class EffectCode
        {
            internal GraphicsLibrary GraphicsLibrary;
            internal CodeType CodeType;
            internal byte[] ByteCode;
        }

        public Effect Effect { get; private set; }

        private List<EffectCode> EffectCodeList;

        public Material()
        {
            EffectCodeList = new List<EffectCode>();

            Effect = null;
        }

        public override bool IsValid => Effect != null;

        public override void Load(string packageName, string assetName, Stream stream)
        {
            EffectCodeList.Clear();

            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true))
            {
                string magic = new string(reader.ReadChars(4));
                AssetVersion = reader.ReadInt32();
                var originalFilename = reader.ReadString();

                int count = reader.ReadInt32();
                EffectCodeList = new List<EffectCode>(count);
                for (int i = 0; i < count; i++)
                {
                    EffectCode code = new EffectCode();
                    code.GraphicsLibrary = (GraphicsLibrary)reader.ReadInt32();
                    code.CodeType = (CodeType)reader.ReadInt32();
                    code.ByteCode = reader.ReadBytes(reader.ReadInt32());
                    EffectCodeList.Add(code);
                }
            }
        }

        private EffectCode GetEffectCode(GraphicsLibrary graphicsLibrary, CodeType codeType)
        {
            return (from e in EffectCodeList
                    where e.GraphicsLibrary == graphicsLibrary &&
                          e.CodeType == codeType
                    select e).FirstOrDefault();
        }

        internal void AddBinaryCode(GraphicsLibrary graphicsLibrary, CodeType codeType, byte[] binaryCode)
        {
            EffectCode existingEntry = GetEffectCode(graphicsLibrary, codeType);
            if (existingEntry != null)
            {
                // TODO Handle error
                return; 
            }


        }

        /// <summary>
        /// Invokes the Shader Compiler tool on disk and re-imports the material
        /// 
        /// Throws a NotImplementedException on macOS and Linux
        /// </summary>
        public static Material CreateFromShaderCode(string code, string setName)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                throw new NotImplementedException("CreateFromShaderCode is not implemented on macOS and Linux");
            }

            Material material = new Material();
            material.Name = setName;
            return material;
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write("AEAF".ToCharArray());
            writer.Write(MaxAssetVersion);
            writer.Write(OriginalFilename);

            writer.Write(EffectCodeList.Count);
            for (int i = 0; i < EffectCodeList.Count; i++)
            {
                writer.Write((int)EffectCodeList[i].GraphicsLibrary);
                writer.Write((int)EffectCodeList[i].CodeType);
                writer.Write((int)EffectCodeList[i].ByteCode.Length);
                writer.Write(EffectCodeList[i].ByteCode);
            }
        }
    }
}
