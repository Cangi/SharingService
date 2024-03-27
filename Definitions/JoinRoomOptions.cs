namespace SharingService
{
    public enum JoinRoomOptions
    {
        /// <summary>
        /// Cancel joining the sharing room/session
        /// </summary>
        Cancel,

        /// <summary>
        /// Join the sharing room/session and bring along the app's current sharable objects.
        /// </summary>
        JoinAndBringObjects,

        /// <summary>
        /// Join the sharing room/session and clear all the app's current sharable objects.
        /// </summary>
        JoinAndClearObjects,
    }
}