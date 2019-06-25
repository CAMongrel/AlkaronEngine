using System;
using System.Collections.Generic;
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
        //private List<BaseActor> Actors;

        public SceneGraph(BaseScene sceneOwner)
        {
            SceneOwner = sceneOwner;
            //Actors = new List<BaseActor>();

            bufferPool = new BepuUtilities.Memory.BufferPool();
            var sceneCallbacks = new SceneCallbacks();
            physicsSimulation = BepuPhysics.Simulation.Create(bufferPool, sceneCallbacks, sceneCallbacks);
        }

        public void Update(double deltaTime)
        {
            // Update phyics
            physicsSimulation.Timestep((float)deltaTime);

            /*for (int i = 0; i < Actors.Count; i++)
            {
                Actors[i].Update(gameTime);
            }

            SceneOwner.RenderManager.SetRenderProxies(GetSceneRenderProxies());*/
        }

        /*public void AddActor(BaseActor newActor)
        {
            if (Actors.Contains(newActor))
            {
                // 
                return;
            }

            Actors.Add(newActor);
            newActor.ActorAddedToSceneGraph(this);

            var physicsShape = newActor.CreatePhysicsShape();
            if (physicsShape != null)
            {
                //physicsSimulation.Shapes.Add(physicsShape ?? new Box()); 
            }

            //SceneOwner.RenderManager.AppendRenderProxies(newActor.CreateRenderProxies());
        }

        public void RemoveActor(BaseActor remActor)
        {
            if (Actors.Contains(remActor) == false)
            {
                // 
                return;
            }

            Actors.Remove(remActor);

            remActor.ActorRemovedFromSceneGraph(this);

            // TODO: Handle physics
            
            //SceneOwner.RenderManager.AppendRenderProxies(newActor.CreateRenderProxies());
        }

        public List<BaseRenderProxy> GetSceneRenderProxies()
        {
            List<BaseRenderProxy> result = new List<BaseRenderProxy>();

            for (int i = 0; i < Actors.Count; i++)
            {
                var list = Actors[i].GetRenderProxies();
                result.AddRange(list);
            }

            return result;
        }*/
    }

    internal unsafe struct SceneCallbacks : INarrowPhaseCallbacks, IPoseIntegratorCallbacks
    {
        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.ConserveMomentum;

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

        public void PrepareForIntegration(float dt)
        {
            
        }

        public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
        {
            
        }
    }
}
