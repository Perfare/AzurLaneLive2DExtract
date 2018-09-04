using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzurLaneLive2DExtract
{
    public class CubismModel3Json
    {
        public int Version;
        public SerializableFileReferences FileReferences;
        public SerializableGroup[] Groups;
    }

    public struct SerializableFileReferences
    {
        public string Moc;
        public string[] Textures;
        public string[] Motions;
        public string Physics;
    }

    public struct SerializableGroup
    {
        public string Target;
        public string Name;
        public string[] Ids;
    }
}
