using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetStudioCore;
using AssetStudioCore.Classes;

namespace AzurLaneLive2DExtract
{
    class CubismMotion3Converter
    {
        private Dictionary<uint, string> bonePathHash = new Dictionary<uint, string>();
        public List<ImportedKeyframedAnimation> AnimationList { get; protected set; } = new List<ImportedKeyframedAnimation>();

        public CubismMotion3Converter(GameObject rootGameObject, AnimationClip[] animationClips)
        {
            var rootTransform = GetTransform(rootGameObject);
            CreateBonePathHash(rootTransform);
            ConvertAnimations(animationClips);
        }

        private void ConvertAnimations(AnimationClip[] animationClips)
        {
            foreach (var animationClip in animationClips)
            {
                var iAnim = new ImportedKeyframedAnimation();
                AnimationList.Add(iAnim);
                iAnim.Name = animationClip.m_Name;
                iAnim.SampleRate = animationClip.m_SampleRate;
                iAnim.Duration = animationClip.m_MuscleClip.m_StopTime;
                var m_Clip = animationClip.m_MuscleClip.m_Clip;
                var streamedFrames = m_Clip.m_StreamedClip.ReadData();
                var m_ClipBindingConstant = animationClip.m_ClipBindingConstant;
                for (int frameIndex = 1; frameIndex < streamedFrames.Count - 1; frameIndex++)
                {
                    var frame = streamedFrames[frameIndex];
                    for (int curveIndex = 0; curveIndex < frame.keyList.Count; curveIndex++)
                    {
                        ReadStreamedData(iAnim, m_ClipBindingConstant, frame.time, frame.keyList[curveIndex]);
                    }
                }
                var m_DenseClip = m_Clip.m_DenseClip;
                var streamCount = m_Clip.m_StreamedClip.curveCount;
                for (int frameIndex = 0; frameIndex < m_DenseClip.m_FrameCount; frameIndex++)
                {
                    var time = m_DenseClip.m_BeginTime + frameIndex / m_DenseClip.m_SampleRate;
                    var frameOffset = frameIndex * m_DenseClip.m_CurveCount;
                    for (int curveIndex = 0; curveIndex < m_DenseClip.m_CurveCount;)
                    {
                        var index = streamCount + curveIndex;
                        ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time, m_DenseClip.m_SampleArray, (int)frameOffset, ref curveIndex);
                    }
                }
                var m_ConstantClip = m_Clip.m_ConstantClip;
                var denseCount = m_Clip.m_DenseClip.m_CurveCount;
                var time2 = 0.0f;
                for (int i = 0; i < 2; i++)
                {
                    for (int curveIndex = 0; curveIndex < m_ConstantClip.data.Length;)
                    {
                        var index = streamCount + denseCount + curveIndex;
                        ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time2, m_ConstantClip.data, 0, ref curveIndex);
                    }
                    time2 = animationClip.m_MuscleClip.m_StopTime;
                }
            }
        }

        private void ReadStreamedData(ImportedKeyframedAnimation iAnim, AnimationClipBindingConstant m_ClipBindingConstant, float time, StreamedClip.StreamedCurveKey curveKey)
        {
            var binding = m_ClipBindingConstant.FindBinding(curveKey.index);
            if (binding.path == 0)
            {
                return;
            }

            GetLive2dPath(binding.path, out var target, out var boneName);
            var track = iAnim.FindTrack(boneName);
            track.Target = target;
            track.Curve.Add(new ImportedKeyframe<float>(time, curveKey.value, curveKey.coeff));
        }

        private void ReadCurveData(ImportedKeyframedAnimation iAnim, AnimationClipBindingConstant m_ClipBindingConstant, int index, float time, float[] data, int offset, ref int curveIndex)
        {
            var binding = m_ClipBindingConstant.FindBinding(index);
            if (binding.path == 0)
            {
                curveIndex++;
                return;
            }

            GetLive2dPath(binding.path, out var target, out var boneName);
            var track = iAnim.FindTrack(boneName);
            track.Target = target;
            var value = data[curveIndex++];
            track.Curve.Add(new ImportedKeyframe<float>(time, value, new[] { 0f, 0f, 0f, value }));
        }

        private void GetLive2dPath(uint path, out string target, out string id)
        {
            id = path.ToString();
            target = path.ToString();
            if (bonePathHash.TryGetValue(path, out var boneName))
            {
                var index = boneName.LastIndexOf('/');
                id = boneName.Substring(index + 1);
                target = boneName.Substring(0, index);
                if (target == "Parameters")
                {
                    target = "Parameter";
                }
                else if (target == "Parts")
                {
                    target = "PartOpacity";
                }
            }
        }

        private Transform GetTransform(GameObject gameObject)
        {
            foreach (var m_Component in gameObject.m_Component)
            {
                var asset = m_Component.Get();
                if (asset.Type == ClassIDReference.Transform)
                {
                    return new Transform(asset);
                }
            }

            return null;
        }

        private void CreateBonePathHash(Transform m_Transform)
        {
            var name = GetTransformPath(m_Transform);
            var crc = new SevenZip.CRC();
            var bytes = Encoding.UTF8.GetBytes(name);
            crc.Update(bytes, 0, (uint)bytes.Length);
            bonePathHash[crc.GetDigest()] = name;
            int index;
            while ((index = name.IndexOf("/", StringComparison.Ordinal)) >= 0)
            {
                name = name.Substring(index + 1);
                crc = new SevenZip.CRC();
                bytes = Encoding.UTF8.GetBytes(name);
                crc.Update(bytes, 0, (uint)bytes.Length);
                bonePathHash[crc.GetDigest()] = name;
            }
            foreach (var pptr in m_Transform.m_Children)
            {
                CreateBonePathHash(new Transform(pptr.Get()));
            }
        }

        private string GetTransformPath(Transform meshTransform)
        {
            var m_GameObject = new GameObject(meshTransform.m_GameObject.Get());
            if (meshTransform.m_Father.TryGet(out var father))
            {
                var transform = new Transform(father);
                return GetTransformPath(transform) + "/" + m_GameObject.m_Name;
            }

            return m_GameObject.m_Name;
        }
    }
}
