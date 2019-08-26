using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Graphics3D.RenderProxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace AlkaronEngine.Components
{
    public class AnimationController
    {
        private double animationPlaybackIndex = 0.0;
        private bool isAnimationActive = false;

        public double PlaybackSpeed { get; set; } = 1.0;

        private BoneAnimationAsset? currentAnimation = null;

        public double CurrentAnimationTime => currentAnimation != null ? animationPlaybackIndex : 0.0;
        public string? CurrentAnimationName => currentAnimation?.AnimationIdentifier;

        public SkeletalMeshComponent Component { get; private set; }

        public AnimationController(SkeletalMeshComponent setComponent)
        {
            Component = setComponent;
        }

        public void Play(string animationIdentifier)
        {
            var anim = GetAnimationByIdentifier(animationIdentifier);
            if (anim == null)
            {
                // TODO: Log error
                return;
            }

            var curAnim = currentAnimation;
            if (curAnim != null)
            {
                // TODO
                // AnimationDidStop()
            }

            // TODO
            // AnimationDidStart()
            animationPlaybackIndex = 0.0;
            currentAnimation = anim;
            isAnimationActive = true;

            Component.SkeletalMesh.ActiveAnimationIdentifier = anim.AnimationIdentifier;
        }

        public void Pause()
        {
            // TODO
            // AnimationDidPause()
            isAnimationActive = false;
        }

        public void Stop()
        {
            // TODO
            // AnimationDidStop()
            isAnimationActive = false;
            animationPlaybackIndex = 0.0;
        }

        public void Resume()
        {
            // TODO
            // AnimationDidStart()
        }

        public void Tick(double deltaTime)
        {
            var anim = currentAnimation;

            if (isAnimationActive == false ||
                anim == null)
            {
                return;
            }

            animationPlaybackIndex += (deltaTime * PlaybackSpeed);
            if (animationPlaybackIndex > anim.AnimationLength)
            {
                double overflow = animationPlaybackIndex - anim.AnimationLength;
                if (overflow > anim.AnimationLength)
                {
                    // Prevent weird scenarios
                    overflow = 0.0;
                }
                animationPlaybackIndex = overflow;
            }
        }

        private BoneAnimationAsset GetAnimationByIdentifier(string identifier)
        {
            identifier = identifier.ToLowerInvariant();
            return (from a in Component.SkeletalMesh.Animations
                    where a.AnimationIdentifier.ToLowerInvariant() == identifier
                    select a).FirstOrDefault();
        }

        public string[] GetAnimationIdentifiers()
        {
            return (from a in Component.SkeletalMesh.Animations
                    select a.AnimationIdentifier).ToArray();
        }
    }

    public class SkeletalMeshComponent : BaseComponent
    {
        //private int frameIndex;
        //private double lastFrameTime;

        public AnimationController AnimationController;

        public SkeletalMesh SkeletalMesh { get; private set; }

        // Reference to the rendering proxy
        //private SkeletalMeshRenderProxy proxy;

        public SkeletalMeshComponent(Vector3 setCenter)
           : base(setCenter)
        {
            //frameIndex = 0;
            CanBeRendered = true;
            AnimationController = new AnimationController(this);
        }

        public void SetSkeletalMesh(SkeletalMesh skeletalMesh)
        {
            SkeletalMesh = skeletalMesh;

            CreateRenderProxies();
        }

        private void CreateRenderProxies()
        {
            List<BaseRenderProxy> resultList = new List<BaseRenderProxy>();

            if (SkeletalMesh != null)
            {
                Matrix4x4 worldMatrix = Matrix4x4.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                       Matrix4x4.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                       Matrix4x4.CreateTranslation(Center);

                SkeletalMeshRenderProxy proxy = new SkeletalMeshRenderProxy(SkeletalMesh);
                proxy.WorldMatrix = worldMatrix;
                proxy.Material = SkeletalMesh.Material;
                proxy.AnimationTime = AnimationController.CurrentAnimationTime;
                proxy.AnimationName = AnimationController.CurrentAnimationName;
                resultList.Add(proxy);
            }

            renderProxies = resultList.ToArray();
        }

        internal override IEnumerable<BaseRenderProxy> GetRenderProxies()
        {
            var proxies = base.GetRenderProxies();
            foreach (var pr in proxies)
            {
                if (pr is SkeletalMeshRenderProxy rp)
                {
                    Matrix4x4 worldMatrix = Matrix4x4.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                       Matrix4x4.CreateScale(Scale.X, Scale.Y, Scale.Z) *
                       Matrix4x4.CreateTranslation(Center);
                    rp.WorldMatrix = worldMatrix;

                    rp.AnimationTime = AnimationController.CurrentAnimationTime;
                    rp.AnimationName = AnimationController.CurrentAnimationName;
                }
            }
            return proxies;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            AnimationController.Tick(deltaTime);
        }
    }
}
