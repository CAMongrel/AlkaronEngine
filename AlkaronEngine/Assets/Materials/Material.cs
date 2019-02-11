using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Util;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Assets.Materials
{
    public class Material : Asset
    {
        protected override int MaxAssetVersion => 2;

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

        public bool RequiresOrderingBackToFront { get; set; }
        public BlendState BlendState { get; set; }
        public SamplerState SamplerState { get; set; }

        public Material()
        {
            RequiresOrderingBackToFront = false;
            EffectCodeList = new List<EffectCode>();
            BlendState = BlendState.Opaque;
            SamplerState = SamplerState.AnisotropicWrap;

            Effect = null;
        }

        public override bool IsValid => Effect != null;

        private void CreateShader()
        {
            GraphicsLibrary graphicsLibrary = AlkaronCoreGame.Core.GraphicsLibrary;
            EffectCode effectCode = GetEffectCode(graphicsLibrary, CodeType.XNAEffect);
            if (effectCode == null)
            {
                Log.Error("Could not load Shader for platform: " + graphicsLibrary + " in Material: " + this.Name);
                return;
            }

            Effect = new Effect(AlkaronCoreGame.Core.GraphicsDevice, effectCode.ByteCode);
        }

        public override void Deserialize(BinaryReader reader)
        {
            EffectCodeList.Clear();

            base.Deserialize(reader);

            if (AssetVersion >= 2)
            {
                RequiresOrderingBackToFront = reader.ReadBoolean();
                int tempBlendState = reader.ReadInt32();
                int tempSamplerState = reader.ReadInt32();
            }

            int count = reader.ReadInt32();
            EffectCodeList = new List<EffectCode>(count);
            for (int i = 0; i < count; i++)
            {
                AddBinaryCode(
                    (GraphicsLibrary)reader.ReadInt32(),
                    (CodeType)reader.ReadInt32(),
                    reader.ReadBytes(reader.ReadInt32()));
            }

            CreateShader();
        }

        private EffectCode GetEffectCode(GraphicsLibrary graphicsLibrary, CodeType codeType)
        {
            return (from e in EffectCodeList
                    where e.GraphicsLibrary == graphicsLibrary &&
                          e.CodeType == codeType
                    select e).FirstOrDefault();
        }

        internal bool AddBinaryCode(GraphicsLibrary graphicsLibrary, CodeType codeType, byte[] binaryCode)
        {
            EffectCode codeEntry = GetEffectCode(graphicsLibrary, codeType);
            if (codeEntry != null)
            {
                // If an entry with this combination already exists, skip it

                // TODO Handle error
                return false; 
            }

            codeEntry = new EffectCode();
            codeEntry.ByteCode = binaryCode;
            codeEntry.CodeType = codeType;
            codeEntry.GraphicsLibrary = graphicsLibrary;
            EffectCodeList.Add(codeEntry);

            if (graphicsLibrary == AlkaronCoreGame.Core.GraphicsLibrary)
            {
                CreateShader(); 
            }

            return true;
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

        internal virtual void SetupEffectForRenderPass(RenderPass renderPass)
        {
            AlkaronCoreGame.Core.GraphicsDevice.SamplerStates[0] = SamplerState;
            AlkaronCoreGame.Core.GraphicsDevice.BlendState = BlendState;
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);

            writer.Write(RequiresOrderingBackToFront);
            writer.Write((Int32)0);     // Placeholder for blend mode
            writer.Write((Int32)0);     // Placeholder for sampler mode

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
