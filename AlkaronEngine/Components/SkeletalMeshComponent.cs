using System;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Components
{
    public class SkeletalMeshComponent : BaseComponent
    {
        public SkeletalMeshComponent(Vector3 setCenter)
           : base(setCenter)
        {
            CanBeRendered = true;
        }
    }
}
