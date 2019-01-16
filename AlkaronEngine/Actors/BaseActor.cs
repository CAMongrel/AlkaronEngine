using System;
using System.Collections.Generic;
using AlkaronEngine.Components;
using AlkaronEngine.Graphics3D.RenderProxies;
using AlkaronEngine.Scene;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Actors
{
    public class BaseActor
    {
        public bool IsAddedToSceneGraph { get; private set; }

        public List<BaseComponent> AttachedComponents { get; private set; }

        public BaseActor()
        {
            AttachedComponents = new List<BaseComponent>();
            IsAddedToSceneGraph = false;
        }

        public virtual void Update(GameTime gameTime)
        {
            for (int i = 0; i < AttachedComponents.Count; i++)
            {
                AttachedComponents[i].Update(gameTime);
            }
        }

        public virtual void ActorAddedToSceneGraph(SceneGraph sceneGraph)
        {
            IsAddedToSceneGraph = true;

            for (int i = 0; i < AttachedComponents.Count; i++)
            {
                AttachedComponents[i].ActorAddedToSceneGraph(sceneGraph);
            }
        }

        internal virtual IShape CreatePhysicsShape()
        {

            return null; 
        }

        public List<BaseRenderProxy> CreateRenderProxies()
        {
            List<BaseRenderProxy> resultList = new List<BaseRenderProxy>();

            for (int i = 0; i < AttachedComponents.Count; i++)
            {
                resultList.AddRange(AttachedComponents[i].CreateRenderProxies());
            }

            return resultList;
        }
    }
}
