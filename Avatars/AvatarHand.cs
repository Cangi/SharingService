// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using RealityCollective.Extensions;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace SharingService.Avatars
{
    /// <summary>
    /// A class holding player's head and joint poses.
    /// </summary>
    public class AvatarHand
    {
        /// <summary>
        /// Get the current joint poses.
        /// </summary>
        public Dictionary<XRHandJointID, Pose> JointPoses { get; } = new Dictionary<XRHandJointID, Pose>
        {
            { AvatarHandDescription.Primary.Joint, Pose.identity },
            { XRHandJointID.ThumbMetacarpal, Pose.identity },
            { XRHandJointID.ThumbProximal, Pose.identity },
            { XRHandJointID.ThumbDistal, Pose.identity },
            { XRHandJointID.ThumbTip, Pose.identity },
            { XRHandJointID.IndexMetacarpal, Pose.identity },
            { XRHandJointID.IndexProximal, Pose.identity },
            { XRHandJointID.IndexIntermediate, Pose.identity },
            { XRHandJointID.IndexDistal, Pose.identity },
            { XRHandJointID.IndexTip, Pose.identity },
            { XRHandJointID.MiddleMetacarpal, Pose.identity },
            { XRHandJointID.MiddleProximal, Pose.identity },
            { XRHandJointID.MiddleIntermediate, Pose.identity },
            { XRHandJointID.MiddleTip, Pose.identity },
            { XRHandJointID.RingMetacarpal, Pose.identity },
            { XRHandJointID.RingProximal, Pose.identity },
            { XRHandJointID.RingIntermediate, Pose.identity },
            { XRHandJointID.RingDistal, Pose.identity },
            { XRHandJointID.RingTip, Pose.identity },
            { XRHandJointID.LittleMetacarpal, Pose.identity },
            { XRHandJointID.LittleProximal, Pose.identity },
            { XRHandJointID.LittleIntermediate, Pose.identity },
            { XRHandJointID.LittleDistal, Pose.identity },
            { XRHandJointID.LittleTip, Pose.identity }
        };

        /// <summary>
        /// Get flags that represent which joint poses are valid.
        /// </summary>
        public AvatarPoseFlag Flags { get; private set; }

        /// <summary>
        /// Reset the change flags.
        /// </summary>
        public void Reset()
        {
            Flags = AvatarPoseFlag.None;
        }

        /// <summary>
        /// Get the change flag for the given joint.
        /// </summary>
        public AvatarPoseFlag GetFlag(XRHandJointID joint)
        {
            return AvatarHandDescription.GetFlag(joint);
        }

        /// <summary>
        /// Get if the joint has changed.
        /// </summary>
        public bool HasJointFlag(XRHandJointID joint)
        {
            return HasFlag(GetFlag(joint));
        }

        /// <summary>
        /// Get if the joint flag is set, indicated a joint change.
        /// </summary>
        private bool HasFlag(AvatarPoseFlag flag)
        {
            if (flag == AvatarPoseFlag.None)
            {
                return false;
            }

            var hasFlag = Flags.HasFlag(flag);
            return hasFlag;
        }

        /// <summary>
        /// Mark the joint has changed, passing in the joint pose. If the joint pose is invalid, the flag is not set.
        /// </summary>
        public void SetJointFlag(ref Pose pose, XRHandJointID joint)
        {
            SetFlag(ref pose, GetFlag(joint));
        }

        /// <summary>
        /// Mark the joint has changed, passing in the joint rotation. If the joint rotation is invalid, the flag is not set.
        /// </summary>
        public void SetJointFlag(ref Quaternion rotation, XRHandJointID joint)
        {
            SetFlag(ref rotation, GetFlag(joint));
        }

        /// <summary>
        /// Mark the joint has changed, passing in the joint pose. If the joint pose is invalid, the flag is not set.
        /// </summary>
        private void SetFlag(ref Pose pose, AvatarPoseFlag flag)
        {
            if (pose.position.IsValidVector() && pose.rotation.IsValidRotation())
            {
                Flags |= flag;
            }
            else if (AvatarPoseFlag.None != flag)
            {
                Flags &= ~flag;
            }
        }

        /// <summary>
        /// Mark the joint has changed, passing in the joint rotation. If the joint rotation is invalid, the flag is not set.
        /// </summary>
        private void SetFlag(ref Quaternion rotation, AvatarPoseFlag flag)
        {
            if (rotation.IsValidRotation())
            {
                Flags |= flag;
            }
            else if (AvatarPoseFlag.None != flag)
            {
                Flags &= ~flag;
            }
        }
    }
}