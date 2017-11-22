using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Components;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Actors;

namespace AlkaronEngine.Scene
{
    public class SceneGraph
    {
        public BaseScene SceneOwner { get; private set; }
        private List<BaseActor> Actors;

        public SceneGraph(BaseScene sceneOwner)
        {
            SceneOwner = sceneOwner;
            Actors = new List<BaseActor>();
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Actors.Count; i++)
            {
                Actors[i].Update(gameTime);
            }
        }

        public void AddActor(BaseActor newActor)
        {
            Actors.Add(newActor);
            newActor.ActorAddedToSceneGraph(this);

            SceneOwner.RenderManager.AppendRenderProxies(newActor.CreateRenderProxies());
        }
    }
}
