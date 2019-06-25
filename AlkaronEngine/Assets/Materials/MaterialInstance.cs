using System;
using System.IO;
using System.Numerics;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Util;
using Veldrid;

namespace AlkaronEngine.Assets.Materials
{
    public enum AlphaMode
    {
        Opaque,
        Mask,
        Blend
    }

    public class MaterialInputs
    {
        public Vector4 DiffuseColor;
        public Surface2D DiffuseTexture;

        public float MetallicFactor;
        public float RoughnessFactor;
        public Surface2D MetallicRoughnessTexture;

        public Vector3 EmissiveFactor;
        public Surface2D EmissiveTexture;

        public Surface2D NormalTexture;
        public Surface2D OcclusionTexture;

        public AlphaMode AlphaMode;
        public float AlphaCutoff;

        public MaterialInputs()
        {
            DiffuseColor = new Vector4(0, 0, 0, 1);
            DiffuseTexture = null;

            MetallicFactor = 0.0f;
            RoughnessFactor = 0.0f;
            MetallicRoughnessTexture = null;

            EmissiveFactor = new Vector3(0, 0, 0);
            EmissiveTexture = null;

            NormalTexture = null;
            OcclusionTexture = null;

            AlphaMode = AlphaMode.Opaque;
            AlphaCutoff = 0.0f;
        }
    }

    public class MaterialInstance : Asset, IMaterial
    {
        public Material Material { get; private set; }
        public MaterialInputs MaterialInputs { get; private set; }

        protected override int MaxAssetVersion => 1;

        public override bool IsValid => Material != null && Material.IsValid;

        public bool RequiresOrderingBackToFront
        {
            get { return Material.RequiresOrderingBackToFront; }
            set { Material.RequiresOrderingBackToFront = value; }
        }

        public MaterialInstance()
            : this(null)
        {
        }

        public MaterialInstance(Material setMaterial)
        {
            MaterialInputs = new MaterialInputs();
            Material = setMaterial;
        }

        public void ApplyParameters(Matrix4x4 worldViewProjectio)
        {
            //var effect = Material.Effect;

            //effect.Parameters["WorldViewProj"].SetValue(worldViewProjectio);
            //effect.CurrentTechnique.Passes[0].Apply();
        }

        public virtual void SetupEffectForRenderPass(RenderPass renderPass)
        {
            //AlkaronCoreGame.Core.GraphicsDevice.SamplerStates[0] = SamplerState;
            //AlkaronCoreGame.Core.GraphicsDevice.BlendState = BlendState;

            //var effect = Material.Effect;

            //effect.Parameters["DiffuseColor"].SetValue(MaterialInputs.DiffuseColor);
            //effect.Parameters["Texture"].SetValue(MaterialInputs.DiffuseTexture.Texture);
        }

        public override void Serialize(BinaryWriter writer, AssetSettings assetSettings)
        {
            base.Serialize(writer, assetSettings);

            // Material reference
            writer.Write(Material);

            // MaterialInputs
            writer.Write(MaterialInputs.DiffuseColor);
            writer.Write(MaterialInputs.DiffuseTexture);

            writer.Write(MaterialInputs.MetallicFactor);
            writer.Write(MaterialInputs.RoughnessFactor);
            writer.Write(MaterialInputs.MetallicRoughnessTexture);

            writer.Write(MaterialInputs.EmissiveFactor);
            writer.Write(MaterialInputs.EmissiveTexture);

            writer.Write(MaterialInputs.NormalTexture);
            writer.Write(MaterialInputs.OcclusionTexture);

            writer.Write((int)MaterialInputs.AlphaMode);
            writer.Write(MaterialInputs.AlphaCutoff);
        }

        public override void Deserialize(BinaryReader reader, AssetSettings assetSettings)
        {
            base.Deserialize(reader, assetSettings);

            // Material reference
            Material = reader.ReadAsset<Material>();

            // MaterialInputs
            MaterialInputs.DiffuseColor = reader.ReadVector4();
            MaterialInputs.DiffuseTexture = reader.ReadAsset<Surface2D>();

            MaterialInputs.MetallicFactor = reader.ReadSingle();
            MaterialInputs.RoughnessFactor = reader.ReadSingle();
            MaterialInputs.MetallicRoughnessTexture = reader.ReadAsset<Surface2D>();

            MaterialInputs.EmissiveFactor = reader.ReadVector3();
            MaterialInputs.EmissiveTexture = reader.ReadAsset<Surface2D>();

            MaterialInputs.NormalTexture = reader.ReadAsset<Surface2D>();
            MaterialInputs.OcclusionTexture = reader.ReadAsset<Surface2D>();

            MaterialInputs.AlphaMode = (AlphaMode)reader.ReadInt32();
            MaterialInputs.AlphaCutoff = reader.ReadSingle();
        }
    }
}
