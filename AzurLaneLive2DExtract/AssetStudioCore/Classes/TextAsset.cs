namespace AssetStudioCore.Classes
{
    public sealed class TextAsset : NamedObject
    {
        public byte[] m_Script;

        public TextAsset(AssetPreloadData preloadData) : base(preloadData)
        {
            m_Script = reader.ReadBytes(reader.ReadInt32());
        }
    }
}
