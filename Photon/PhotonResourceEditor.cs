using RealityCollective.Extensions;
using UnityEngine;

namespace SharingService.Photon
{
#if UNITY_EDITOR
    public class PhotonResourceEditor : SharingServiceResourceEditor
    {
        public PhotonResourceEditor() : base(SharingProviderType.PhotonPun)
        { }

        protected override void InitializeVariant(GameObject variant)
        {
#if PHOTON_INSTALLED
            var view = variant.EnsureComponent<PhotonViewExtended>();
            view.OwnershipTransfer = global::Photon.Pun.OwnershipOption.Takeover;
            view.observableSearch = global::Photon.Pun.PhotonView.ObservableSearch.Manual;
#endif
        }
    }
#endif
}