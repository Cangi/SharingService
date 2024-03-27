// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace SharingService
{
    public struct SharingServicePlayerData
    {
        public SharingServicePlayerData(string displayName, SharingServicePlayerStatus status, string playerId,
            bool isLocal)
        {
            DisplayName = displayName;
            Status = status;
            PlayerId = playerId;
            IsLocal = isLocal;
        }

        /// <summary>
        /// Get the display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Get the status
        /// </summary>
        public SharingServicePlayerStatus Status { get; private set; }

        /// <summary>
        /// The id of this player, for the current sharing room/session.
        /// </summary>
        public string PlayerId { get; private set; }

        /// <summary>
        /// Get if this player is local
        /// </summary>
        public bool IsLocal { get; private set; }
    }
}