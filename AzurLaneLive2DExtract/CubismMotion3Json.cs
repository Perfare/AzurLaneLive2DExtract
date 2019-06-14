using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurLaneLive2DExtract
{
    public class CubismMotion3Json
    {
        public int Version;
        public SerializableMeta Meta;
        public SerializableCurve[] Curves;
        public SerializableUserData[] UserData;
    }

    public class SerializableMeta
    {
        public float Duration;
        public float Fps;
        public bool Loop;
        public int CurveCount;
        public int TotalSegmentCount;
        public int TotalPointCount;
        public int UserDataCount;
        public int TotalUserDataSize;
    };

    public class SerializableCurve
    {
        public string Target;
        public string Id;
        public List<float> Segments;
    };

    public class SerializableUserData
    {
        public float Time;
        public string Value;
    }
}
