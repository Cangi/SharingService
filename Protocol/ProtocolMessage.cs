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