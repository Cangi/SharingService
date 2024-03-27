namespace SharingService
{
    /// <summary>
    /// An enumeration describing the type of message
    /// </summary>
    public enum ProtocolMessageType : byte
    {
        Unknown = 0,
        PropertyChanged = 1,
        Command = 2,
        SharingServiceTransform = 4,
        SharingServiceMessage = 5,
        SharingServicePingRequest = 7,
        SharingServicePingResponse = 8,
        SharingServiceSpawnParameter = 9,
    }
}