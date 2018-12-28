using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Components;
using AlkaronEngine.Graphics3D.RenderProxies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Actors;
using System.Runtime.CompilerServices;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Collidables;
using BepuPhysics;

namespace AlkaronEngine.Scene
{
    public class SceneGraph
    {
        private BepuPhysics.Simulation physicsSimulation;
        private BepuUtilities.Memory.BufferPool bufferPool;

        public BaseScene SceneOwner { get; private set; }
        private List<BaseActor> Actors;

        public SceneGraph(BaseScene sceneOwner)
        {
            SceneOwner = sceneOwner;
            Actors = new List<BaseActor>();

            bufferPool = new BepuUtilities.Memory.BufferPool();
            physicsSimulation = BepuPhysics.Simulation.Create(bufferPool, new SceneCallbacks());
        }

        public void Update(GameTime gameTime)
        {
            // Update phyics
            physicsSimulation.Timestep((float)gameTime.ElapsedGameTime.TotalSeconds);

            for (int i = 0; i < Actors.Count; i++)
            {
                Actors[i].Update(gameTime);
            }
        }

        public void AddActor(BaseActor newActor)
        {
            Actors.Add(newActor);
            newActor.ActorAddedToSceneGraph(this);

            var physicsShape = newActor.CreatePhysicsShape();
            if (physicsShape != null)
            {
                //physicsSimulation.Shapes.Add(physicsShape ?? new Box()); 
            }

            SceneOwner.RenderManager.AppendRenderProxies(newActor.CreateRenderProxies());
        }
    }

    internal unsafe struct SceneCallbacks : BepuPhysics.CollisionDetection.INarrowPhaseCallbacks
    {
        public void Initialize(Simulation simulation)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ConfigureMaterial(out PairMaterialProperties pairMaterial)
        {
            pairMaterial.FrictionCoefficient = 1f;
            pairMaterial.MaximumRecoveryVelocity = 2f;
            pairMaterial.SpringSettings = new BepuPhysics.Constraints.SpringSettings(30, 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold(int workerIndex, CollidablePair pair, NonconvexContactManifold* manifold, out PairMaterialProperties pairMaterial)
        {
            ConfigureMaterial(out pairMaterial);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold(int workerIndex, CollidablePair pair, ConvexContactManifold* manifold, out PairMaterialProperties pairMaterial)
        {
            ConfigureMaterial(out pairMaterial);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ConvexContactManifold* manifold)
        {
            return true;
        }

        public void Dispose()
        {
        }
    }
}
