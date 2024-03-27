#if PHOTON_INSTALLED

namespace SharingService.Photon
{
    public struct PhotonMessage
    {
        public PhotonParticipant sender;

        public ProtocolMessage inner;
    }
}
#endif // PHOTON_INSTALLED