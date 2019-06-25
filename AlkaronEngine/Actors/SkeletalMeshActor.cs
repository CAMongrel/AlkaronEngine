using System;
using System.Numerics;
using AlkaronEngine.Components;

namespace AlkaronEngine.Actors
{
    public class SkeletalMeshActor : BaseActor
    {
        public SkeletalMeshComponent SkeletalMeshComponent { get; private set; }

        public SkeletalMeshActor()
        {
            SkeletalMeshComponent = new SkeletalMeshComponent(Vector3.Zero);

            AttachComponent(SkeletalMeshComponent);
        }
    }
}
