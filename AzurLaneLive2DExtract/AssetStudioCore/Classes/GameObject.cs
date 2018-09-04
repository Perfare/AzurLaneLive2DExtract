using System.Collections.Generic;

namespace AssetStudioCore.Classes
{
    public sealed class GameObject : EditorExtension
    {
        public List<PPtr> m_Component;
        public string m_Name;

        public GameObject(AssetPreloadData preloadData) : base(preloadData)
        {
            int m_Component_size = reader.ReadInt32();
            m_Component = new List<PPtr>(m_Component_size);
            for (int j = 0; j < m_Component_size; j++)
            {
                if ((version[0] == 5 && version[1] >= 5) || version[0] > 5)//5.5.0 and up
                {
                    m_Component.Add(sourceFile.ReadPPtr());
                }
                else
                {
                    int first = reader.ReadInt32();
                    m_Component.Add(sourceFile.ReadPPtr());
                }
            }
            var m_Layer = reader.ReadInt32();
            m_Name = reader.ReadAlignedString();
        }
    }
}
