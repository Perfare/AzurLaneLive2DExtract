using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudioCore.Classes
{
    public abstract class Object
    {
        public AssetsFile sourceFile;
        protected EndianBinaryReader reader;
        protected int[] version;
        protected string[] buildType;
        protected BuildTarget platform;


        protected Object(AssetPreloadData preloadData)
        {
            sourceFile = preloadData.sourceFile;
            reader = preloadData.InitReader();
            version = sourceFile.version;
            buildType = sourceFile.buildType;
            platform = (BuildTarget)sourceFile.platform;

            if (platform == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
        }
    }
}
