// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace SharingService.Avatars
{
    // <summary>
    /// This describes what hand data is serialized and send to other clients.
    /// </summary>
    public class AvatarHandDescription
    {
        private FingerSerializationType _fingerSerialization = FingerSerializationType.None;

        /// <summary>
        /// A cache that stores the all finger joints.
        /// </summary>
        private static AvatarJointDescription[] _allJoints = null;

        /// <summary>
        /// A cache that stores the serialized finger joints.
        /// </summary>
        private AvatarJointDescription[] _serializableJoints = null;

        /// <summary>
        /// Describes how to change a joint to its corresponding flag.
        /// </summary>
        private static Dictionary<XRHandJointID, AvatarPoseFlag> _jointToFlag = new Dictionary<XRHandJointID, AvatarPoseFlag>()
        {
            { XRHandJointID.Wrist, AvatarPoseFlag.Hand },
            { XRHandJointID.Palm, AvatarPoseFlag.Hand },
            { XRHandJointID.ThumbMetacarpal, AvatarPoseFlag.None },
            { XRHandJointID.ThumbProximal, AvatarPoseFlag.ThumbProximal },
            { XRHandJointID.ThumbDistal, AvatarPoseFlag.ThumbDistal },
            { XRHandJointID.ThumbTip, AvatarPoseFlag.ThumbTip },
            { XRHandJointID.IndexMetacarpal, AvatarPoseFlag.None },
            { XRHandJointID.IndexProximal, AvatarPoseFlag.IndexProximal },
            { XRHandJointID.IndexIntermediate, AvatarPoseFlag.IndexMiddle },
            { XRHandJointID.IndexDistal, AvatarPoseFlag.IndexDistal },
            { XRHandJointID.IndexTip, AvatarPoseFlag.IndexTip },
            { XRHandJointID.MiddleMetacarpal, AvatarPoseFlag.None },
            { XRHandJointID.MiddleProximal, AvatarPoseFlag.MiddleProximal },
            { XRHandJointID.MiddleIntermediate, AvatarPoseFlag.MiddleMiddle },
            { XRHandJointID.MiddleDistal, AvatarPoseFlag.MiddleDistal },
            { XRHandJointID.MiddleTip, AvatarPoseFlag.MiddleTip },
            { XRHandJointID.RingMetacarpal, AvatarPoseFlag.None },
            { XRHandJointID.RingProximal, AvatarPoseFlag.RingProximal },
            { XRHandJointID.RingIntermediate, AvatarPoseFlag.RingMiddle },
            { XRHandJointID.RingDistal, AvatarPoseFlag.RingDistal },
            { XRHandJointID.RingTip, AvatarPoseFlag.RingTip },
            { XRHandJointID.LittleMetacarpal, AvatarPoseFlag.None },
            { XRHandJointID.LittleProximal, AvatarPoseFlag.LittleProximal },
            { XRHandJointID.LittleIntermediate, AvatarPoseFlag.PinkyMiddle },
            { XRHandJointID.LittleDistal, AvatarPoseFlag.PinkyDistal },
            { XRHandJointID.LittleTip, AvatarPoseFlag.LittleTip }
        };

        /// <summary>
        /// Describes how to change a flag to its corresponding join.
        /// </summary>
        private static Dictionary<AvatarPoseFlag, XRHandJointID> _flagToJoint = new Dictionary<AvatarPoseFlag, XRHandJointID>();

        /// <summary>
        /// Describes how to change a joint enum to a description.
        /// </summary>
        private static Dictionary<XRHandJointID, AvatarJointDescription> _jointToDescription = new Dictionary<XRHandJointID, AvatarJointDescription>();

        /// <summary>
        /// Describes the Unity bone type of left hand joints.
        /// </summary>
        private static Dictionary<XRHandJointID, HumanBodyBones> _leftBones = new Dictionary<XRHandJointID, HumanBodyBones>()
        {
            { XRHandJointID.Wrist, HumanBodyBones.LeftHand },
            { XRHandJointID.Palm, HumanBodyBones.LeftHand },
            { XRHandJointID.ThumbMetacarpal, HumanBodyBones.LeftThumbProximal },
            { XRHandJointID.ThumbProximal, HumanBodyBones.LeftThumbIntermediate },
            { XRHandJointID.ThumbDistal, HumanBodyBones.LeftThumbDistal },
            { XRHandJointID.IndexProximal, HumanBodyBones.LeftIndexProximal },
            { XRHandJointID.IndexIntermediate, HumanBodyBones.LeftIndexIntermediate },
            { XRHandJointID.IndexDistal, HumanBodyBones.LeftIndexDistal },
            { XRHandJointID.MiddleProximal, HumanBodyBones.LeftMiddleProximal },
            { XRHandJointID.MiddleIntermediate, HumanBodyBones.LeftMiddleIntermediate },
            { XRHandJointID.MiddleDistal, HumanBodyBones.LeftMiddleDistal },
            { XRHandJointID.RingProximal, HumanBodyBones.LeftRingProximal },
            { XRHandJointID.RingIntermediate, HumanBodyBones.LeftRingIntermediate },
            { XRHandJointID.RingDistal, HumanBodyBones.LeftRingDistal },
            { XRHandJointID.LittleProximal, HumanBodyBones.LeftLittleProximal },
            { XRHandJointID.LittleIntermediate, HumanBodyBones.LeftLittleIntermediate },
            { XRHandJointID.LittleDistal, HumanBodyBones.LeftLittleDistal }
        };

        /// <summary>
        /// Describes the Unity bone type of left hand joints.
        /// </summary>
        private static Dictionary<XRHandJointID, HumanBodyBones> _rightBones = new Dictionary<XRHandJointID, HumanBodyBones>()
        {
            { XRHandJointID.Wrist, HumanBodyBones.RightHand },
            { XRHandJointID.Palm, HumanBodyBones.RightHand },
            { XRHandJointID.ThumbMetacarpal, HumanBodyBones.RightThumbProximal },
            { XRHandJointID.ThumbProximal, HumanBodyBones.RightThumbIntermediate },
            { XRHandJointID.ThumbDistal, HumanBodyBones.RightThumbDistal },
            { XRHandJointID.IndexProximal, HumanBodyBones.RightIndexProximal },
            { XRHandJointID.IndexIntermediate, HumanBodyBones.RightIndexIntermediate },
            { XRHandJointID.IndexDistal, HumanBodyBones.RightIndexDistal },
            { XRHandJointID.MiddleProximal, HumanBodyBones.RightMiddleProximal },
            { XRHandJointID.MiddleIntermediate, HumanBodyBones.RightMiddleIntermediate },
            { XRHandJointID.MiddleDistal, HumanBodyBones.RightMiddleDistal },
            { XRHandJointID.RingProximal, HumanBodyBones.RightRingProximal },
            { XRHandJointID.RingIntermediate, HumanBodyBones.RightRingIntermediate },
            { XRHandJointID.RingDistal, HumanBodyBones.RightRingDistal },
            { XRHandJointID.LittleProximal, HumanBodyBones.RightLittleProximal },
            { XRHandJointID.LittleIntermediate, HumanBodyBones.RightLittleIntermediate },
            { XRHandJointID.LittleDistal, HumanBodyBones.RightLittleDistal }
        };

        /// <summary>
        /// The finger tips of a hand
        /// </summary>
        private static HashSet<XRHandJointID> _fingerTips = new HashSet<XRHandJointID>()
        {
            XRHandJointID.ThumbTip,
            XRHandJointID.IndexTip,
            XRHandJointID.MiddleTip,
            XRHandJointID.RingTip,
            XRHandJointID.LittleTip,
        };

        /// <summary>
        /// The finger joints of a hand
        /// </summary>
        private static Dictionary<AvatarFinger, XRHandJointID[]> _fingerJoints = new Dictionary<AvatarFinger, XRHandJointID[]>()
        {
            { AvatarFinger.Thumb, new XRHandJointID[] { XRHandJointID.ThumbMetacarpal, XRHandJointID.ThumbProximal, XRHandJointID.ThumbDistal, XRHandJointID.ThumbTip } },
            { AvatarFinger.Index, new XRHandJointID[] { XRHandJointID.IndexProximal, XRHandJointID.IndexIntermediate, XRHandJointID.IndexDistal, XRHandJointID.IndexTip } },
            { AvatarFinger.Middle, new XRHandJointID[] { XRHandJointID.MiddleProximal, XRHandJointID.MiddleIntermediate, XRHandJointID.MiddleDistal, XRHandJointID.MiddleTip } },
            { AvatarFinger.Ring, new XRHandJointID[] { XRHandJointID.RingProximal, XRHandJointID.RingIntermediate, XRHandJointID.RingDistal, XRHandJointID.RingTip } },
            { AvatarFinger.Little, new XRHandJointID[] { XRHandJointID.LittleProximal, XRHandJointID.LittleIntermediate, XRHandJointID.LittleDistal, XRHandJointID.LittleTip } },
        };

        static AvatarHandDescription()
        {
            foreach (var entry in _jointToFlag)
            {
                if (!_flagToJoint.ContainsKey(entry.Value))
                {
                    _flagToJoint[entry.Value] = entry.Key;
                }
            }
        }

        /// <summary>
        /// Get or set if finger tips should be serialized
        /// </summary>
        public FingerSerializationType FingerSerializationType
        {
            get => _fingerSerialization;

            set
            {
                if (_fingerSerialization != value)
                {
                    _fingerSerialization = value;
                    _serializableJoints = null;
                }
            }
        }

        /// <summary>
        /// Get all hand joints, even those that aren't serialized
        /// </summary>
        public static AvatarJointDescription[] AllJoints
        {
            get
            {
                if (_allJoints == null)
                {
                    var jointToDescription = new Dictionary<XRHandJointID, AvatarJointDescription>(_jointToFlag.Count);

                    // Add hand joint first
                    jointToDescription[Primary.Joint] = Primary;

                    foreach (var entry in _jointToFlag)
                    {
                        var joint = entry.Key;
                        var flag = entry.Value;

                        // Can't use joints with a None flag, and the Hand joint was already added
                        if (flag == AvatarPoseFlag.Hand)
                        {
                            continue;
                        }

                        AvatarJointDescription description;
                        if (_fingerTips.Contains(joint))
                        {
                            description = new AvatarJointDescription(
                                joint,
                                flag,
                                hasPose: true);
                        }
                        else
                        {
                            if (_leftBones.TryGetValue(joint, out var leftBone) &&
                                _rightBones.TryGetValue(joint, out var rightBone))
                            {
                                description = new AvatarJointDescription(
                                    joint,
                                    flag,
                                    leftBone,
                                    rightBone,
                                    hasPose: false);
                            }
                            else
                            {
                                description = new AvatarJointDescription(
                                    joint,
                                    flag,
                                    hasPose: false);
                            }
                        }

                        jointToDescription[description.Joint] = description;
                    }

                    _allJoints = jointToDescription.Values.ToArray();
                    _jointToDescription = jointToDescription;

                }

                return _allJoints;
            }
        }

        /// <summary>
        /// Get the joint description of the primary hand joint that is serialized and sent to other clients.
        /// </summary>
        /// <remarks>
        /// If this is changed, you'll likely have to update your avatar model.
        /// </remarks>
        public static AvatarJointDescription Primary { get; } = new AvatarJointDescription(
            XRHandJointID.Wrist,
            _jointToFlag[XRHandJointID.Wrist],
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand,
            hasPose: true);

        /// <summary>
        /// Get the joint description of the primary hand joint that is serialized and sent to other clients.
        /// </summary>
        /// <remarks>
        /// If this is changed, you'll likely have to update your avatar model.
        /// </remarks>
        public AvatarJointDescription SerializableJoint => Primary;

        /// <summary>
        /// Get the descriptions of the joints that are serialized and sent to other clients.
        /// </summary>
        public AvatarJointDescription[] SerializableJoints
        {
            get
            {
                if (_serializableJoints == null)
                {
                    var allJoints = AllJoints;
                    var serializableJoints = new List<AvatarJointDescription>(allJoints.Length);
                    foreach (var entry in allJoints)
                    {
                        // Can't serialize something without a change flag
                        if (entry.Flag == AvatarPoseFlag.None)
                        {
                            continue;
                        }

                        bool fingerTip = _fingerTips.Contains(entry.Joint);
                        if ((entry.IsHand) ||
                            (_fingerSerialization == FingerSerializationType.FingerTips && fingerTip) || 
                            (_fingerSerialization == FingerSerializationType.JointRotations && !fingerTip))
                        {
                            serializableJoints.Add(entry);
                        }
                    }
                    _serializableJoints = serializableJoints.ToArray();
                }
                return _serializableJoints;
            }
        }

        /// <summary>
        /// Get the joint's change flag.
        /// </summary>
        public static AvatarPoseFlag GetFlag(XRHandJointID joint)
        {
            return _jointToFlag[joint];
        }

        /// <summary>
        /// Get the flag's joint description.
        /// </summary>
        public static AvatarJointDescription GetJointDescription(AvatarPoseFlag flag)
        {
            return _jointToDescription[_flagToJoint[flag]];
        }

        /// <summary>
        /// Get the finger joints.
        /// </summary>
        public static XRHandJointID[] GetJoints(AvatarFinger finger)
        {
            return _fingerJoints[finger];
        }
    }

    /// <summary>
    /// Describes the finger serialization.
    /// </summary>
    public enum FingerSerializationType
    {
        [Tooltip("Serialization is not defined.")]
        Unknown = 0,

        [Tooltip("No finger data is serialized.")]
        None = 1,

        [Tooltip("Only the finger tips will be serialized.")]
        FingerTips = 2,

        [Tooltip("Only the finger joints will be serialized.")]
        JointRotations = 3
    }
}