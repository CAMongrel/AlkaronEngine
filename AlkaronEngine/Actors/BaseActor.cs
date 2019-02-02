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

        private List<BaseComponent> attachedComponents;

        public BaseActor()
        {
            attachedComponents = new List<BaseComponent>();
            IsAddedToSceneGraph = false;
        }

        internal virtual void Update(GameTime gameTime)
        {
            for (int i = 0; i < attachedComponents.Count; i++)
            {
                attachedComponents[i].Update(gameTime);
            }
        }

        internal virtual void ActorAddedToSceneGraph(SceneGraph sceneGraph)
        {
            IsAddedToSceneGraph = true;

            for (int i = 0; i < attachedComponents.Count; i++)
            {
                attachedComponents[i].OwnerActorAddedToSceneGraph(sceneGraph);
            }
        }

        internal virtual void ActorRemovedFromSceneGraph(SceneGraph sceneGraph)
        {
            IsAddedToSceneGraph = false;
        }

        internal virtual IShape CreatePhysicsShape()
        {

            return null; 
        }

        internal IEnumerable<BaseRenderProxy> GetRenderProxies()
        {
            List<BaseRenderProxy> resultList = new List<BaseRenderProxy>();

            for (int i = 0; i < attachedComponents.Count; i++)
            {
                var componentProxies = attachedComponents[i].GetRenderProxies();

                // Some components may have no rendering component
                if (componentProxies != null)
                {
                    resultList.AddRange(componentProxies);
                }
            }

            return resultList;
        }

        public void AttachComponent(BaseComponent component)
        {
            if (attachedComponents.Contains(component))
            {
                return;
            }

            attachedComponents.Add(component);
        }

        public void DetachComponent(BaseComponent component)
        {
            if (attachedComponents.Contains(component) == false)
            {
                return;
            }

            attachedComponents.Remove(component);
        }
    }
}
