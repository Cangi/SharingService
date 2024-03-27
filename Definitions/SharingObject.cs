using RealityCollective.ServiceFramework.Services;

namespace SharingService
{
    /// <summary>
    /// A SharingObject component that represents a root parent, and is capable of sharing state for
    /// this game object and its children.
    /// </summary>
    public class SharingObject : SharingObjectBase
    {
        #region Public Properties
        /// <summary>
        /// Get if this is a root
        /// </summary>
        public override sealed bool IsRoot => true;
        #endregion Public Properties

        #region MonoBehaviour Functions
        /// <summary>
        /// Ensure the in-scene components are added to the object
        /// </summary>
        private void OnValidate()
        {
            if (ServiceManager.Instance != null && ServiceManager.Instance.TryGetService(out ISharingService sharingService))
            {
                sharingService.EnsureNetworkObjectComponents(gameObject);
            }
            
        }

        /// <summary>
        /// Add in other sharing components that are needed
        /// </summary>
        private void Awake()
        {
            if (ServiceManager.Instance.TryGetService(out ISharingService sharingService))
            {
                sharingService.EnsureNetworkObjectComponents(gameObject);
            }
        }
        #endregion MonoBehavior Functions
    }
}