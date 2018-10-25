using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics2D
{
    public class MaterialLibrary
    {
        private Dictionary<string, Material> materials;
        private IRenderConfiguration renderConfig;

        public static readonly string DefaultMaterialName = "defaultmaterial";

        public MaterialLibrary(IRenderConfiguration setRenderConfig)
        {
            renderConfig = setRenderConfig;
            materials = new Dictionary<string, Material>();

            CreateAndAddDefaultMaterial();
        }

        private void CreateAndAddDefaultMaterial()
        {
            BasicEffect basicEffect = new BasicEffect(renderConfig.GraphicsDevice);
            basicEffect.LightingEnabled = false;
            basicEffect.FogEnabled = false;
            basicEffect.TextureEnabled = false;
            basicEffect.VertexColorEnabled = true;

            Material defaultMat = new Material(renderConfig);
            defaultMat.SetEffect(basicEffect);
            defaultMat.BlendState = BlendState.Opaque;
            materials.Add(DefaultMaterialName, defaultMat);
        }

        public void AddMaterial(string name, Material material)
        {
            name = name.ToLowerInvariant();

            if (name == DefaultMaterialName)
            {
                // Do not allow any changes to the default material
                return;
            }

            if (materials.ContainsKey(name))
            {
                materials[name] = material;
            }
            else
            {
                materials.Add(name, material);
            }
        }

        public void RemoveMaterial(string name)
        {
            name = name.ToLowerInvariant();

            if (name == DefaultMaterialName)
            {
                // Do not allow any changes to the default material
                return;
            }

            if (materials.ContainsKey(name))
            {
                materials.Remove(name);
            }
        }

        public Material GetMaterialByName(string name)
        {
            name = name.ToLowerInvariant();

            if (materials.ContainsKey(name))
            {
                return materials[name];
            }
            else
            {
                return null;
            }
        }
    }
}
