// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Hands;
using Handedness = UnityEngine.XR.Hands.Handedness;

namespace SharingService.Avatars
{
   public class AvatarHandTransforms : MonoBehaviour
    {
          private Matrix4x4 _sourceToWorld = Matrix4x4.identity;
        private Dictionary<Transform, Transform> _ownedTransforms = null;
        private Dictionary<XRHandJointID, Transform> _pendingTransforms = null;
        private Dictionary<XRHandJointID, Transform> _transforms = 
            new Dictionary<XRHandJointID, Transform>();

        #region Serialized Fields
        [Header("General")]

        [SerializeField]
        [Tooltip("The handedness")]
        private Handedness handedness = Handedness.Left;

        /// <summary>
        /// The hand renderer.
        /// </summary>
        public Handedness Handedness
        {
            get => handedness;
            set => handedness = value;
        }

        [SerializeField]
        [Tooltip("Create copies of transforms")]
        private bool copyTransforms = false;

        /// <summary>
        /// Create copies of transforms
        /// </summary>
        public bool CopyTransforms
        {
            get => copyTransforms;
            set => copyTransforms = value;
        }

        [Header("Head Transform")]

        [SerializeField]
        [Tooltip("The head. This is only used during copy operation, so that the copied wrist is placed relative to the head. If null, parent transform is used.")]
        private Transform head = null;

        /// <summary>
        /// The head. This is only used during copy operation, so that the copied wrist is placed relative to the head. If null, parent transform is used.
        /// </summary>
        public Transform Head
        {
            get => head;
            set => head = value;
        }

        [Header("Joint Transforms")]

        [SerializeField]
        [Tooltip("The wrist root.")]
        private Transform wrist = null;

        /// <summary>
        /// Get or set the wrist root.
        /// </summary>
        public Transform Wrist
        {
            get => wrist;
            set => wrist = value;
        }

        [SerializeField]
        [Tooltip("The thumb root, at the metacarpal.")]
        private Transform thumbMetacarpal = null;

        /// <summary>
        /// The thumb root, at the metacarpal.
        /// </summary>
        public Transform ThumbMetacarpal
        {
            get => thumbMetacarpal;
            set => thumbMetacarpal = value;
        }

        [FormerlySerializedAs("indexKnuckle")]
        [SerializeField]
        [Tooltip("The index finger root, at the knuckle.")]
        private Transform indexProximal = null;

        /// <summary>
        /// The index finger root, at the knuckle.
        /// </summary>
        public Transform IndexKnuckle
        {
            get => indexProximal;
            set => indexProximal = value;
        }

        [FormerlySerializedAs("middleKnuckle")]
        [SerializeField]
        [Tooltip("The middle finger root, at the knuckle.")]
        private Transform middleProximal = null;

        /// <summary>
        /// The middle finger root, at the knuckle.
        /// </summary>
        public Transform MiddleKnuckle
        {
            get => middleProximal;
            set => middleProximal = value;
        }

        [FormerlySerializedAs("ringKnuckle")]
        [SerializeField]
        [Tooltip("The ring finger root, at the knuckle.")]
        private Transform ringProximal = null;

        /// <summary>
        /// The ring finger root, at the knuckle.
        /// </summary>
        public Transform RingKnuckle
        {
            get => ringProximal;
            set => ringProximal = value;
        }

        [FormerlySerializedAs("pinkyKnuckle")]
        [SerializeField]
        [Tooltip("The pinky finger root, at the knuckle.")]
        private Transform littleProximal = null;

        /// <summary>
        /// The pinky finger root, at the knuckle.
        /// </summary>
        public Transform PinkyKnuckle
        {
            get => littleProximal;
            set => littleProximal = value;
        }

        [Header("Joint Animator")]

        [SerializeField]
        [Tooltip("The animator used to extract joints from. If null the hand hierarchy is used instead.")]
        private Animator animator = null;

        /// <summary>
        /// The animator used to extract joint transforms from. If null the hand hierarchy is used instead.
        /// </summary>
        public Animator Animator
        {
            get => animator;
            set
            {
                if (animator != value)
                {
                    animator = value;
                    Initialize();
                }
            }
        }
        #endregion Serialized Fields

        #region Public Properties 
        public IReadOnlyDictionary<XRHandJointID, Transform> Transforms => _transforms;
        #endregion Public Properties

        #region Public Events
        public event Action<AvatarHandTransforms, IReadOnlyDictionary<XRHandJointID, Transform>> TransformsChanged;
        #endregion Public Events

        #region MonoBehavior Functions
        private void Start()
        {
            Initialize();
        }
        #endregion MonoBehavior Functions

        #region Public Functions
        public void FindTransforms()
        {
            Initialize();
        }
        #endregion Public Functions 

        #region Private Functions
        /// <summary>
        /// Initialize the transforms
        /// </summary>
        private void Initialize()
        {
            _pendingTransforms = new Dictionary<XRHandJointID, Transform>();
            if (animator != null)
            {
                InitializeJointTransformsFromAnimator();
            }
            else
            {
                InitializeJointTransformsFromHierarchy();
            }

            DestroyCopies();
            if (copyTransforms)
            {
                _pendingTransforms = CreateCopy(_pendingTransforms);
            }

            _transforms = _pendingTransforms;
            TransformsChanged?.Invoke(this, _transforms);

        }

        /// <summary>
        /// Initialize the joint transforms from hierarchy
        /// </summary>
        private void InitializeJointTransformsFromHierarchy()
        {
            // if wrist is copied, we need to know the relative root position so to place it.
            // we assume this parent is the destination head.
            if (transform.parent == null || head == null)
            {
                _sourceToWorld = Matrix4x4.identity;
            }
            else
            {
                _sourceToWorld = transform.parent.localToWorldMatrix * head.worldToLocalMatrix;
            }

            // primary hand transform
            InitializeJointTransform(AvatarHandDescription.Primary.Joint, wrist);

            // thumb transforms
            InitializeJointTransform(XRHandJointID.ThumbMetacarpal, thumbMetacarpal);
            InitializeJointTransform(XRHandJointID.ThumbProximal, RetrieveChildTransform(XRHandJointID.ThumbMetacarpal));
            InitializeJointTransform(XRHandJointID.ThumbDistal, RetrieveChildTransform(XRHandJointID.ThumbProximal));
            InitializeJointTransform(XRHandJointID.ThumbTip, RetrieveChildTransform(XRHandJointID.ThumbDistal));

            // index finger transforms
            InitializeJointTransform(XRHandJointID.IndexProximal, indexProximal);
            InitializeJointTransform(XRHandJointID.IndexIntermediate, RetrieveChildTransform(XRHandJointID.IndexProximal));
            InitializeJointTransform(XRHandJointID.IndexDistal, RetrieveChildTransform(XRHandJointID.IndexIntermediate));
            InitializeJointTransform(XRHandJointID.IndexTip, RetrieveChildTransform(XRHandJointID.IndexDistal));

            // middle finger transforms
            InitializeJointTransform(XRHandJointID.MiddleProximal, middleProximal);
            InitializeJointTransform(XRHandJointID.MiddleIntermediate, RetrieveChildTransform(XRHandJointID.MiddleProximal));
            InitializeJointTransform(XRHandJointID.MiddleDistal, RetrieveChildTransform(XRHandJointID.MiddleIntermediate));
            InitializeJointTransform(XRHandJointID.MiddleTip, RetrieveChildTransform(XRHandJointID.MiddleDistal));

            // ring finger transforms
            InitializeJointTransform(XRHandJointID.RingProximal, ringProximal);
            InitializeJointTransform(XRHandJointID.RingIntermediate, RetrieveChildTransform(XRHandJointID.RingProximal));
            InitializeJointTransform(XRHandJointID.RingDistal, RetrieveChildTransform(XRHandJointID.RingIntermediate));
            InitializeJointTransform(XRHandJointID.RingTip, RetrieveChildTransform(XRHandJointID.RingDistal));

            // pinky transforms
            InitializeJointTransform(XRHandJointID.LittleProximal, littleProximal);
            InitializeJointTransform(XRHandJointID.LittleIntermediate, RetrieveChildTransform(XRHandJointID.LittleProximal));
            InitializeJointTransform(XRHandJointID.LittleDistal, RetrieveChildTransform(XRHandJointID.LittleIntermediate));
            InitializeJointTransform(XRHandJointID.LittleTip, RetrieveChildTransform(XRHandJointID.LittleDistal));
        }

        /// <summary>
        /// Initialize the joint transforms from a given animator
        /// </summary>
        private void InitializeJointTransformsFromAnimator()
        {
            if (animator == null)
            {
                return;
            }

            // if wrist is copied, we need to know the relative root position so to place it.
            // we assume this parent is the destination head, and the source head is defined in the animator
            var sourceRoot = animator.GetBoneTransform(HumanBodyBones.Head);
            if (sourceRoot == null || transform.parent == null)
            {
                _sourceToWorld = Matrix4x4.identity;
            }
            else
            {
                _sourceToWorld = transform.parent.localToWorldMatrix * sourceRoot.worldToLocalMatrix;
            }

            foreach (var joint in AvatarHandDescription.AllJoints)
            {
                if (joint.HasBone)
                {
                    Transform jointTransform = animator.GetBoneTransform(joint.Bone(handedness));
                    if (jointTransform != null)
                    {
                        InitializeJointTransform(joint.Joint, jointTransform);
                    }
                }
            }

            // The finger tips don't have bones, so search the hierarchy for these
            InitializeJointTransform(XRHandJointID.ThumbTip, RetrieveChildTransform(XRHandJointID.ThumbDistal));
            InitializeJointTransform(XRHandJointID.IndexTip, RetrieveChildTransform(XRHandJointID.IndexDistal));
            InitializeJointTransform(XRHandJointID.MiddleTip, RetrieveChildTransform(XRHandJointID.MiddleDistal));
            InitializeJointTransform(XRHandJointID.RingTip, RetrieveChildTransform(XRHandJointID.RingDistal));
            InitializeJointTransform(XRHandJointID.LittleTip, RetrieveChildTransform(XRHandJointID.LittleDistal));
        }

        /// <summary>
        /// Add joint transform to the joint dictionary
        /// </summary>
        private void InitializeJointTransform(XRHandJointID joint, Transform jointTransform)
        {
            if (jointTransform != null)
            {
                _pendingTransforms[joint] = jointTransform;
            }
        }

        private void DestroyCopies()
        {
            if (_ownedTransforms != null)
            {
                foreach (var entry in _ownedTransforms)
                {
                    if (entry.Value != null)
                    {
                        Destroy(entry.Value.gameObject);
                    }
                }
                _ownedTransforms = null;
            }
        }

        private Dictionary<XRHandJointID, Transform> CreateCopy(Dictionary<XRHandJointID, Transform> originalTransforms)
        {
            _ownedTransforms = new Dictionary<Transform, Transform>();
            var result = new Dictionary<XRHandJointID, Transform>(originalTransforms.Count);
            foreach (var entry in originalTransforms)
            {
                result[entry.Key] = CreateCopy(entry.Value);
            }
            return result;
        }

        private Transform CreateCopy(Transform jointTransform)
        {
            Transform parentCopy;
            if (jointTransform.parent == null ||
                !_ownedTransforms.TryGetValue(jointTransform.parent, out parentCopy))
            {
                parentCopy = transform;
            }
            var copy = new GameObject($"{jointTransform.name} (copy)");
            copy.transform.SetParent(parentCopy);

            var pose = _sourceToWorld * jointTransform.localToWorldMatrix;
            copy.transform.SetPositionAndRotation(
                new Vector3(pose[0, 3], pose[1, 3], pose[2, 3]),
                pose.rotation);

            _ownedTransforms[jointTransform] = copy.transform;
            return copy.transform;
        }

        private Transform RetrieveChildTransform(XRHandJointID parentJoint)
        {
            _pendingTransforms.TryGetValue(parentJoint, out Transform jointTransform);

            if (jointTransform != null && jointTransform.childCount > 0)
            {
                return jointTransform.GetChild(0);
            }
            return null;
        }
        #endregion Private Functions
    }
}
