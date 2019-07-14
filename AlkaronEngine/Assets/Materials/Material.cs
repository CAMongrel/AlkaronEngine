using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Util;
using Veldrid;

namespace AlkaronEngine.Assets.Materials
{
    public class Material : Asset, IMaterial
    {
        protected override int MaxAssetVersion => 1;

        public bool RequiresOrderingBackToFront { get; set; }

        public Material()
        {
            RequiresOrderingBackToFront = false;
        }

        public override bool IsValid => false;

        private void CreateShader()
        {
            GraphicsLibrary graphicsLibrary = AlkaronCoreGame.Core.GraphicsLibrary;
        }

        internal override void Deserialize(BinaryReader reader, AssetSettings assetSettings)
        {
            base.Deserialize(reader, assetSettings);

            RequiresOrderingBackToFront = reader.ReadBoolean();

            CreateShader();
        }

        public void ApplyParameters(Matrix4x4 worldViewProjectio)
        {
            //Effect.Parameters["WorldViewProj"].SetValue(worldViewProjectio);
            //Effect.CurrentTechnique.Passes[0].Apply();
        }

        internal override void Serialize(BinaryWriter writer, AssetSettings assetSettings)
        {
            base.Serialize(writer, assetSettings);

            writer.Write(RequiresOrderingBackToFront);
        }

        public void SetupEffectForRenderPass(RenderPass renderPass)
        {
            //throw new NotImplementedException();
        }
    }
}
