﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudioCore.Classes
{
    public abstract class Texture : NamedObject
    {
        protected Texture(AssetPreloadData preloadData) : base(preloadData)
        {
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                var m_ForcedFallbackFormat = reader.ReadInt32();
                var m_DownscaleFallback = reader.ReadBoolean();
                reader.AlignStream(4);
            }
        }
    }
}
