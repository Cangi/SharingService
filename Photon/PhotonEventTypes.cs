#if PHOTON_INSTALLED

namespace SharingService.Photon
{
    /// <summary>
    /// The Photon custom event types
    /// </summary>
    public enum PhotonEventTypes
    {
        PlayerPoseEvent = 198,
        ProtocolMessageEvent = 199,
        Max = 199
    }
}
#endif