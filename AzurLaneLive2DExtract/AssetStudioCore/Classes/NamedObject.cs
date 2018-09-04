using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudioCore.Classes
{
    public abstract class NamedObject : EditorExtension
    {
        public string m_Name;

        protected NamedObject(AssetPreloadData preloadData) : base(preloadData)
        {
            m_Name = reader.ReadAlignedString();
        }
    }
}
