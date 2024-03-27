// Licensed under the MIT License. See LICENSE in the project root for license information.

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