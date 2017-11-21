using System;
using AlkaronEngine.Components;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Actors
{
    public class StaticMeshActor : BaseActor
    {
        public StaticMeshComponent StaticMeshComponent { get; private set; }

        public StaticMeshActor()
        {
            StaticMeshComponent = new StaticMeshComponent(Vector3.Zero);

            AttachedComponents.Add(StaticMeshComponent);
        }
    }
}
