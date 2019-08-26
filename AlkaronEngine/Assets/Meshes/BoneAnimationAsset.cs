using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AlkaronEngine.Assets.Meshes
{
    public class AnimationFrame<T> where T : struct
    {
        public double timecode = 0.0;
        public T value = default(T);
    }

    public class AnimationBoneData
    {
        public int boneIndex;

        public AnimationFrame<Vector3>[] TranslationFrames;
        public AnimationFrame<Quaternion>[] RotationFrames;
        public AnimationFrame<Vector3>[] ScalingFrames;

        public Matrix4x4 GetMatrix(double timeIndex)
        {
            Vector3 translation = Vector3.Zero;
            Quaternion rotation = Quaternion.Identity;
            Vector3 scale = Vector3.One;

            if (TranslationFrames != null &&
                TranslationFrames.Length > 0)
            {
                if (timeIndex < TranslationFrames[0].timecode)
                {
                    translation = TranslationFrames[0].value;
                }
                else
                {
                    for (int i = 1; i < TranslationFrames.Length; i++)
                    {
                        var preFrame = TranslationFrames[i - 1];
                        var postFrame = TranslationFrames[i];

                        if (timeIndex >= preFrame.timecode &&
                            timeIndex <= postFrame.timecode)
                        {
                            float lerpVal = (float)(timeIndex - preFrame.timecode);
                            translation = Vector3.Lerp(preFrame.value, postFrame.value, lerpVal);
                            break;
                        }
                    }
                }
            }
            if (RotationFrames != null &&
                RotationFrames.Length > 0)
            {
                if (timeIndex < RotationFrames[0].timecode)
                {
                    rotation = RotationFrames[0].value;
                }
                else
                {
                    for (int i = 1; i < RotationFrames.Length; i++)
                    {
                        var preFrame = RotationFrames[i - 1];
                        var postFrame = RotationFrames[i];

                        if (timeIndex >= preFrame.timecode &&
                            timeIndex <= postFrame.timecode)
                        {
                            float lerpVal = (float)(timeIndex - preFrame.timecode);
                            rotation = Quaternion.Lerp(preFrame.value, postFrame.value, lerpVal);
                            break;
                        }
                    }
                }
            }
            if (ScalingFrames != null &&
                ScalingFrames.Length > 0)
            {
                if (timeIndex < ScalingFrames[0].timecode)
                {
                    scale = ScalingFrames[0].value;
                }
                else
                {
                    for (int i = 1; i < ScalingFrames.Length; i++)
                    {
                        var preFrame = ScalingFrames[i - 1];
                        var postFrame = ScalingFrames[i];

                        if (timeIndex >= preFrame.timecode &&
                            timeIndex <= postFrame.timecode)
                        {
                            float lerpVal = (float)(timeIndex - preFrame.timecode);
                            scale = Vector3.Lerp(preFrame.value, postFrame.value, lerpVal);
                            break;
                        }
                    }
                }
            }

            return Matrix4x4.CreateScale(scale) *
                Matrix4x4.CreateFromQuaternion(rotation) *
                Matrix4x4.CreateTranslation(translation);
        }
    }

    // Asset for a single animation using bone structures
    public class BoneAnimationAsset : Asset
    {
        public override bool IsValid => true;

        public string AnimationIdentifier { get; set; } = string.Empty;

        public double AnimationStart { get; internal set; }
        public double AnimationLength { get; internal set; }

        private List<AnimationBoneData> bones = new List<AnimationBoneData>();

        internal void AddBone(AnimationBoneData bone)
        {
            bones.Add(bone);
        }

        public AnimationBoneData DataByBoneIndex(int boneIndex)
        {
            return (from b in bones
                    where b.boneIndex == boneIndex
                    select b).FirstOrDefault();
        }
    }
}
