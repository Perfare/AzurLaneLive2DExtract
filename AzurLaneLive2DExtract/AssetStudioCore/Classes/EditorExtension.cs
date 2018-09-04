using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudioCore.Classes
{
    public abstract class EditorExtension : Object
    {
        protected EditorExtension(AssetPreloadData preloadData) : base(preloadData)
        {
            if (platform == BuildTarget.NoTarget)
            {
                var m_PrefabParentObject = sourceFile.ReadPPtr();
                var m_PrefabInternal = sourceFile.ReadPPtr();
            }
        }
    }
}
