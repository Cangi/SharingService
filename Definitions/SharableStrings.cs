// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace SharingService
{
     /// <summary>
    /// A set of strings used for property names and event 
    /// </summary>
    public static class SharableStrings
    {
        /// <summary>
        /// A property name for sharing the player's username for display.
        /// </summary>
        public const string PlayerName = "playername";

        /// <summary>
        /// A property name for sharing if the player's primary avatar color.
        /// </summary>
        public const string PlayerPrimaryColor = "primarycolor";

        /// <summary>
        /// a property name for sharing latency to another player
        /// </summary>
        public const string PlayerLatency = "latency";
        
        /// <summary>
        /// A command notifying that a object should delete itself.
        /// </summary>
        public const string CommandObjectDelete = "deleted";

        /// <summary>
        /// A property name for sharing the object's serialized model data.
        /// </summary>
        public const string ObjectData = "data";

        /// <summary>
        /// A property name for sharing if an object is in the process of being deleted.
        /// </summary>
        public const string ObjectIsDeleting = "deleting";

        /// <summary>
        /// A property name for sharing if an object is enabled and visible.
        /// </summary>
        public const string ObjectIsEnabled = "enabled";

        /// <summary>
        /// A property name for sharing an object's local transform.
        /// </summary>
        public const string ObjectTransform = "transform";
    }
}