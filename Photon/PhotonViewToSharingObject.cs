#if PHOTON_INSTALLED
using Photon.Pun;
using RealityCollective.ServiceFramework.Services;
using UnityEngine;

namespace SharingService.Photon
{
    public class PhotonViewToSharingObject : MonoBehaviour, IPunInstantiateMagicCallback
    {
        #region MonoBehaviour Functions
        private void OnValidate()
        {
            InitializeObject();
        }

        private void Awake()
        {
            InitializeObject();
        }
        #endregion MonoBehaviour Functions

        #region IPunInstantiateMagicCallback Functions
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            InitializeObject();
        }
        #endregion IPunInstantiateMagicCallback Functions

        #region Private Functions
        /// <summary>
        /// If a static scene object, copy net id to sharing service target.
        /// </summary>
        private void InitializeObject()
        {
            var photonView = GetComponent<PhotonView>();
            var sharingObject = GetComponent<SharingObjectBase>();
            if (photonView != null && sharingObject != null && photonView.HasValidId() && sharingObject.IsRoot)
            {
                sharingObject.Label = photonView.ViewID.ToString();
                if (Application.isPlaying)
                {
                    var initializer = ServiceManager.Instance.GetService<ISharingService>() as ISharingServiceObjectInitializer;
                    initializer?.InitializeSharingObject(sharingObject, photonView.InstantiationData);
                }
            }
        }
        #endregion Private Functions
    }
}
#endif // PHOTON_INSTALLED