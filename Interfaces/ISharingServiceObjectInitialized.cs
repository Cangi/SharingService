namespace SharingService
{
    public interface ISharingServiceObjectInitialized
    {
        /// <summary>
        /// Invoked when the sharing object has been initialized
        /// </summary>
        void Initialized(ISharingServiceObject target, object[] data);
    }
}