// Licensed under the MIT License. See LICENSE in the project root for license information.

#if PHOTON_INSTALLED
using RealityCollective.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace SharingService.Photon
{
   /// <summary>
    /// Ensures that all the required mesh components are available in the scene.
    /// </summary>
    public class PhotonComponents
    {
        private GameObject _root;
        private GameObject _components;
        private GameObject _rootComponents;
        private LogHelper<PhotonComponents> _logger = new LogHelper<PhotonComponents>();

        #region Public Properties
        /// <summary>
        /// Get or set the container for all sharing related game objects.
        /// </summary>
        public GameObject Root
        {
            get => _root;

            set
            {
                if (_root != value)
                {
                    _root = value;

                    if (Components != null)
                    {
                        MoveToRoot(Components.transform);
                    }
                }
            }
        }

        /// <summary>
        /// Components that need to be at the "session origin" will placed on the component
        /// </summary>
        public GameObject Components
        {
            get => _components;

            private set
            {
                if (_components != value)
                {
                    _components = value;

                    if (_components != null)
                    {
                        MoveToRoot(_components.transform);
                    }
                }
            }
        }

        /// <summary>
        /// Components that need to be at the "app root" will placed on the component
        /// </summary>
        public GameObject RootComponents
        {
            get => _rootComponents;

            private set
            {
                if (_rootComponents != value)
                {
                    _rootComponents = value;
                }
            }
        }
        #endregion Public Properties

        #region Constructor
        private PhotonComponents(SharingServiceProfile profile, GameObject root)
        {
            _root = root;
            _logger.Verbose = profile.VerboseLogging ? LogHelperState.Always : LogHelperState.Default;
        }
        #endregion Constructor

        #region Public Functions
        public static PhotonComponents Create(SharingServiceProfile profile, GameObject root)
        {
            var result = new PhotonComponents(profile, root);
            result.EnsureDynamicComponentsContainer(active: false);
            result.EnsureDynamicComponentsContainer(active: true);
            return result;
        }
        #endregion Public Functions

        #region Private Functions
        /// <summary>
        /// A helper to move the given transform to the root game object.
        /// </summary>
        private void MoveToRoot(Transform moveThis)
        {
            if (moveThis != null && moveThis.gameObject != Root)
            {
                if (Root != null)
                {
                    moveThis.transform.SetParent(Root.transform, false);
                }
                else
                {
                    moveThis.transform.SetParent(null, false);
                }
            }
        }

        /// <summary>
        /// Create a container for placing dynamically created components.
        /// </summary>
        private void EnsureDynamicComponentsContainer(bool active)
        {
            if (Components == null)
            {
                Components = new GameObject("PhotonDynamicComponents");
            }

            if (RootComponents == null)
            {
                RootComponents = new GameObject("PhotonDynamicRootComponents");
            }

            Components.SetActive(active);
            RootComponents.SetActive(active);
        }
        


        /// <summary>
        /// Delete components of type T from the Unity scene.
        /// </summary>
        private bool DeleteComponent<T>() where T : Component
        {
            var component = UnityEngine.Object.FindObjectOfType<T>(includeInactive: true);
            if (component != null)
            {
                UnityEngine.Object.Destroy(component);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Ensure the scene has the given component. If missing this type gets added to the given playground.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="playground"></param>
        /// <returns></returns>
        private T EnsureComponent<T>(GameObject playground) where T : Component
        {
            return UnityEngine.Object.FindObjectOfType<T>(includeInactive: true) ?? playground.EnsureComponent<T>();
        }

        /// <summary>
        /// Get if the given unity event has a present event handler with the given name and target.
        /// </summary>
        private bool HasCallback<T, U>(UnityEvent<T, U> unityEvent, UnityEngine.Object target, string methodName)
        {
            bool hasCallback = false;
            if (unityEvent != null)
            {
                int handlerCount = unityEvent.GetPersistentEventCount();
                for (int i = 0; i < handlerCount; i++)
                {
                    if (unityEvent.GetPersistentMethodName(i) == methodName &&
                        unityEvent.GetPersistentTarget(i) == target)
                    {
                        hasCallback = true;
                    }
                }
            }
            return hasCallback;
        }
        #endregion Private Region
    }
}
#endif // PHOTON_INSTALL