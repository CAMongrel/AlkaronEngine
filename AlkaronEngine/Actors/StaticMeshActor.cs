using System;
using System.Numerics;
using AlkaronEngine.Components;

namespace AlkaronEngine.Actors
{
    public class StaticMeshActor : BaseActor
    {
        public StaticMeshComponent StaticMeshComponent { get; private set; }

        public StaticMeshActor()
        {
            StaticMeshComponent = new StaticMeshComponent(Vector3.Zero);

            AttachComponent(StaticMeshComponent);
        }
    }
}
