using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudioCore
{
    public class PPtr
    {
        public AssetsFile sourceFile;
        public int m_FileID;
        public long m_PathID;

        public AssetPreloadData Get()
        {
            return sourceFile.preloadTable[m_PathID];
        }

        public bool TryGet(out AssetPreloadData assetPreloadData )
        {
            return sourceFile.preloadTable.TryGetValue(m_PathID, out assetPreloadData);
        }
    }

    public static class PPtrHelpers
    {
        public static PPtr ReadPPtr(this AssetsFile sourceFile)
        {
            var result = new PPtr();
            result.sourceFile = sourceFile;
            var reader = sourceFile.reader;
            result.m_FileID = reader.ReadInt32();
            result.m_PathID = sourceFile.fileGen < 14 ? reader.ReadInt32() : reader.ReadInt64();
            return result;
        }
    }
}
