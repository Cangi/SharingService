using UnityEngine;

namespace SharingService
{
    /// <summary>
    /// Application GameObject Extensions
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Is the game object a prefab game object, this only functions in editor
        /// </summary>
        public static bool IsPrefab(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

#if UNITY_EDITOR
            return
                UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null ||
                UnityEditor.EditorUtility.IsPersistent(gameObject);
#else
        return false;
#endif
        }
    }
}