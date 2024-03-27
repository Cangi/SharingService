#if PHOTON_INSTALLED
using Photon.Pun;

namespace SharingService.Photon
{
    public static class PhotonViewExtensions
    {
        /// <summary>
        /// Get if the view id is valid.
        /// </summary>
        public static bool HasValidId(this PhotonView view)
        {
            return view != null && view.ViewID > 0;
        }
    }
}
#endif