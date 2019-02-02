using System;
using AlkaronEngine.Components;
using Microsoft.Xna.Framework;

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
