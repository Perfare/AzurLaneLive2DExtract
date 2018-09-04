namespace AssetStudioCore.Classes
{
    public sealed class Animator : Behaviour
    {
        public PPtr m_Avatar;
        public PPtr m_Controller;

        public Animator(AssetPreloadData preloadData) : base(preloadData)
        {
            m_Avatar = sourceFile.ReadPPtr();
            m_Controller = sourceFile.ReadPPtr();
        }
    }
}
