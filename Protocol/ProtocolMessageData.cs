namespace SharingService
{
    /// <summary>
    /// A struct to hold the sharing service protocol message data and data type.
    /// </summary>
    public struct ProtocolMessageData
    {
        /// <summary>
        /// The type of the message data
        /// </summary>
        public ProtocolMessageDataType type;

        /// <summary>
        /// The data itself
        /// </summary>
        public object value;
    }
}