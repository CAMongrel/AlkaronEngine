using System;
using System.Collections.Generic;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;

namespace AlkaronEngine.Graphics3D.Geometry
{
    /// <summary>
    /// Represents a SkeletalMesh where the bones are baked into the Vertices.
    /// Animations are implemented by creating a series of StaticMeshes for each
    /// AnimationFrame.
    /// </summary>
    public class BakedSkeletalMesh
    {
        public Material Material { get; set; }
        public BoundingBox BoundingBox { get; set; }

        internal class AnimationFrame
        {
            internal int Speed;
            internal StaticMesh[] Meshes;
        }

        internal class Animation
        {
            private AnimationFrame[] frames;
        }

        private List<Animation> animations;
        private Animation currentAnimation;

        public BakedSkeletalMesh()
        {
            animations = new List<Animation>();
            currentAnimation = null;
        }

        public void AddAnimationFrame(string name, int speed, StaticMesh[] meshes)
        {
            /*AnimationFrame frame = new AnimationFrame();
            frame.Speed = speed;
            frame.Meshes = meshes;
            frames.Add(frame);*/
        }

        public void RemoveAnimationFrame(string name)
        {
            
        }
    }
}
