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

            if (TranslationFrames != null)
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
            if (RotationFrames != null)
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
            if (ScalingFrames != null)
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

        public double AnimationLength { get; private set; }

        private List<AnimationBoneData> bones = new List<AnimationBoneData>();

        public AnimationBoneData DataByBoneIndex(int boneIndex)
        {
            return (from b in bones
                    where b.boneIndex == boneIndex
                    select b).FirstOrDefault();
        }
    }
}
