// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace SharingService
{
    /// <summary>
    /// The networking service used to share data. Currently only Photon is supported.
    /// </summary>
    public enum SharingProviderType
    {
        None = 0,
        Offline = 1,
        PhotonPun = 2,
    }
}