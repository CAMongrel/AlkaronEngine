using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class EffectLibrary
   {
      private Dictionary<string, Effect> effects;

      public EffectLibrary()
      {
         effects = new Dictionary<string, Effect>();
      }

      public void AddEffect(string name, Effect effect)
      {
         name = name.ToLowerInvariant();

         if (effects.ContainsKey(name))
         {
            effects[name] = effect;
         } else
         {
            effects.Add(name, effect);  
         }
      }

      public Effect EffectByName(string name)
      {
         name = name.ToLowerInvariant();

         if (effects.ContainsKey(name))
         {
            return effects[name];
         } else
         {
            return null;
         }
      }
   }
}
