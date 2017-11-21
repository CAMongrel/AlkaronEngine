﻿using System;
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

        public void Draw(GameTime gameTime, RenderManager renderManager)
        {
            // TODO
            /*Vector3 cameraWorldLocation = renderManager.CameraLocation;

            renderManager.ClearRenderPasses();
            Dictionary<Material, RenderPass> renderPassDict = new Dictionary<Material, RenderPass>();

            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].CanBeRendered == false)
                {
                    continue;
                }

                BaseRenderProxy[] proxies = Components[i].Draw(gameTime, renderManager);
                if (proxies == null || proxies.Length == 0)
                {
                    continue;
                }

                for (int p = 0; p < proxies.Length; p++)
                {
                    BaseRenderProxy proxy = proxies[p];

                    RenderPass passToUse = null;

                    if (renderPassDict.ContainsKey(proxy.Material) == false)
                    {
                        passToUse = renderManager.CreateAndAddRenderPassForMaterial(proxy.Material);
                        renderPassDict.Add(proxy.Material, passToUse);
                    }
                    else
                    {
                        passToUse = renderPassDict[proxy.Material];
                    }

                    passToUse.WorldOriginForDepthSorting = cameraWorldLocation;

                    passToUse.AddProxy(proxy);
                }
            }*/
        }
    }
}
