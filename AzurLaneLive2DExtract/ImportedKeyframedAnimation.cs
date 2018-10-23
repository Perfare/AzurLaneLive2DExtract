using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace AzurLaneLive2DExtract
{
    public class ImportedKeyframedAnimation
    {
        public string Name { get; set; }
        public float SampleRate { get; set; }
        public float Duration { get; set; }

        public List<ImportedAnimationKeyframedTrack> TrackList { get; set; } = new List<ImportedAnimationKeyframedTrack>();

        public ImportedAnimationKeyframedTrack FindTrack(string name)
        {
            var track = TrackList.Find(x => x.Name == name);
            if (track == null)
            {
                track = new ImportedAnimationKeyframedTrack { Name = name };
                TrackList.Add(track);
            }
            return track;
        }
    }

    public class ImportedKeyframe<T>
    {
        public float time { get; set; }
        public T value { get; set; }
        public float[] coeff { get; set; }

        public ImportedKeyframe(float time, T value, float[] coeff)
        {
            this.time = time;
            this.value = value;
            this.coeff = coeff;
        }

        public float Evaluate(float sampleTime)
        {
            float t = sampleTime - time;
            return (t * (t * (t * coeff[0] + coeff[1]) + coeff[2])) + coeff[3];
        }
    }

    public class ImportedAnimationKeyframedTrack
    {
        public string Name { get; set; }
        public string Target { get; set; }
        public List<ImportedKeyframe<float>> Curve = new List<ImportedKeyframe<float>>();
    }
}
