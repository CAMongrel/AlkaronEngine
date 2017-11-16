using System;
using System.Collections.Generic;

namespace AlkaronEngine.Graphics2D
{
   public class MaterialLibrary
   {
      private Dictionary<string, Material> materials;

      public MaterialLibrary()
      {
         materials = new Dictionary<string, Material>();
      }

      public void AddMaterial(string name, Material material)
      {
         name = name.ToLowerInvariant();

         if (materials.ContainsKey(name))
         {
            materials[name] = material;
         }
         else
         {
            materials.Add(name, material);
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
