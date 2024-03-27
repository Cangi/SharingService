#if PHOTON_INSTALLED

using Photon.Realtime;
using System.Collections.Generic;

namespace SharingService.Photon
{
    public class PhotonHelpers 
    {
        private static Dictionary<int, string> _idStrings = new Dictionary<int, string>();
        
        /// <summary>
        /// Convert a Photon user id to a string.
        /// </summary>
        public static string UserIdToString(Player player)
        {
            if (player == null)
            {
                return null;
            }
            else
            {
                return UserIdToString(player.ActorNumber);
            }
        }

        /// <summary>
        /// Convert a Photon user id to a string.
        /// </summary>
        public static string UserIdToString(int id)
        {
            if (id < 0)
            {
                return null;
            }
            else
            {
                string result;
                if (!_idStrings.TryGetValue(id, out result))
                {
                    result = id.ToString();
                    _idStrings[id] = result;
                }
                return result;
            }
        }

        /// <summary>
        /// Convert a string to a Photon user id
        /// </summary>
        public static int UserIdFromString(string stringId)
        {
            int id = -1;
            if (string.IsNullOrEmpty(stringId) || !int.TryParse(stringId, out id) || id < 0)
            {
                return -1;
            }
            else
            {
                return id;
            }
        }
    }
}
#endif