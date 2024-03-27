namespace SharingService
{
    public interface ISharingServiceObjectInitializer
    {
        /// <summary>
        /// Initialize the sharing service object.
        /// </summary>
        void InitializeSharingObject(ISharingServiceObject sharingObject, object[] data);
    }

}