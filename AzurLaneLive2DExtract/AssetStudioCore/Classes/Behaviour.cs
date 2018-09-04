using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudioCore.Classes
{
    public abstract class Behaviour : Component
    {
        protected Behaviour(AssetPreloadData preloadData) : base(preloadData)
        {
            var m_Enabled = reader.ReadByte();
            reader.AlignStream(4);
        }
    }
}
