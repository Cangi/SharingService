using UnityEngine;
using UnityEngine.XR.Hands;

namespace SharingService.Avatars
{
    public class AvatarJointDescription
    {
        HumanBodyBones _leftBone;
        HumanBodyBones _rightBone;

        /// <summary>
        /// Get the joint value
        /// </summary>
        public XRHandJointID Joint { get; }

        /// <summary>
        /// Get the joint change flag
        /// </summary>
        public AvatarPoseFlag Flag { get; }

        /// <summary>
        /// Is this the primary hand pose
        /// </summary>
        public bool IsHand => Flag == AvatarPoseFlag.Hand;

        /// <summary>
        /// Get if the pose should be serialized for this joint
        /// </summary>
        public bool HasPose { get; }

        /// <summary>
        /// Get if joint rotates a bone
        /// </summary>
        public bool HasBone { get; }

        public AvatarJointDescription(
            XRHandJointID joint, 
            AvatarPoseFlag flag, 
            HumanBodyBones leftBone,
            HumanBodyBones rightBone,
            bool hasPose)
        {
            Joint = joint;
            Flag = flag;
            HasPose = hasPose;
            HasBone = true;

            _leftBone = leftBone;
            _rightBone = rightBone;
        }

        public AvatarJointDescription(
            XRHandJointID joint,
            AvatarPoseFlag flag,
            bool hasPose)
        {
            Joint = joint;
            Flag = flag;
            HasPose = hasPose;
            HasBone = false;
        }

        public HumanBodyBones Bone(Handedness hand)
        {
            return hand == Handedness.Left ? _leftBone : _rightBone;
        }
    }
}