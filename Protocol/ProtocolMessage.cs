// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace SharingService
{
    /// <summary>
    /// A struct to hold the sharing service message type and data
    /// </summary>
    public struct ProtocolMessage
    {
        /// <summary>
        /// The type of the message
        /// </summary>
        public ProtocolMessageType type;

        /// <summary>
        /// The message data
        /// </summary>
        public ProtocolMessageData data;
    }
}