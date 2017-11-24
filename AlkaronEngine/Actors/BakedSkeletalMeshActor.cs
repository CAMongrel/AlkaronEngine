using System;
using AlkaronEngine.Components;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Actors
{
    public class BakedSkeletalMeshActor : BaseActor
    {
        public BakedSkeletalMeshComponent BakedSkeletalMeshComponent { get; private set; }

        public BakedSkeletalMeshActor()
        {
            BakedSkeletalMeshComponent = new BakedSkeletalMeshComponent(Vector3.Zero);

            AttachedComponents.Add(BakedSkeletalMeshComponent);
        }
    }
}
