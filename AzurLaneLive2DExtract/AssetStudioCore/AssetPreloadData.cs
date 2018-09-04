using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudioCore
{
    public class AssetPreloadData
    {
        public long m_PathID;
        public uint Offset;
        public int Size;
        public ClassIDReference Type;
        public int Type1;
        public int Type2;

        public string TypeString;

        public AssetsFile sourceFile;
        public string uniqueID;

        public EndianBinaryReader InitReader()
        {
            var reader = sourceFile.reader;
            reader.Position = Offset;
            return reader;
        }
    }
}
