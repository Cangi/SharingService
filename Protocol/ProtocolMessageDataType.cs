// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace SharingService
{
    /// <summary>
    /// An enumeration describing the type of the message data
    /// </summary>
    public enum ProtocolMessageDataType : byte
    {
        Unknown = 0,

        // Built in data types
        Boolean = 1,
        Short = 2,
        Int = 3,
        Float = 4,
        String = 6,
        Long = 7,

        // object types
        Guid = 10,
        DateTime = 11,
        TimeSpan = 12,
        Color = 13,

        // Special case data
        SharingServiceTransform = 20,

        // Custom data
        SharingServicePlayerPose = 251,
        SharingServicePingRequest = 252,
        SharingServicePingResponse = 253,
        SharingServiceMessage = 254,
    }
}