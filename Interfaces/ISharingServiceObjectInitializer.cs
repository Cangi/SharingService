// Licensed under the MIT License. See LICENSE in the project root for license information.

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