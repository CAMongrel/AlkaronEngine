using System;
using System.Collections.Generic;
using AlkaronEngine.Components;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Actors
{
    public class BaseActor
    {
        public List<BaseComponent> AttachedComponents { get; private set; }

        public BaseActor()
        {
            AttachedComponents = new List<BaseComponent>();
        }

        public virtual void Update(GameTime gameTime)
        {
            for (int i = 0; i < AttachedComponents.Count; i++)
            {
                AttachedComponents[i].Update(gameTime);
            }
        }
    }
}
