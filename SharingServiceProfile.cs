using System.Collections.Generic;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SharingService
{
    [CreateAssetMenu(menuName = "SharingServiceProfile", fileName = "SharingServiceProfile", order = (int)CreateProfileMenuItemIndices.ServiceConfig)]
    public class SharingServiceProfile : BaseServiceProfile<IServiceModule>
    {
        public SharingProviderType ProviderType;
        public string sharingId;
        public bool AutoStart;
        public bool VerboseLogging;
        public Color[] PlayerColors = null;
        [Tooltip("The sharing service avatar prefab")]
        public GameObject AvatarPrefab = null;
        [Tooltip("The format of the new public room names. The {0} field will be filled with an integer.")]
        public string RoomNameFormat = "Room {0}";
        public List<string> CustomProperties = new List<string>();
#if UNITY_EDITOR
        private void OnValidate()
        {
            SharingPropertiesBuilder.CreatePropertiesEnumFromConstants(CustomProperties);
        }
#endif
        
    }
}
