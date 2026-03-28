using Unity.Netcode;
using UnityEngine;
using Colocation.Core;

namespace Colocation.Network
{
    /// <summary>
    /// Syncs a player's avatar (head + hands) across AR/VR clients
    /// using colocation-relative coordinates.
    ///
    /// ┌─────────────────────────────────────────────────────────┐
    /// │  SETUP (on the NetworkObject player prefab):            │
    /// │                                                         │
    /// │  PlayerRoot (NetworkObject + PlayerColocationSync)      │
    /// │    ├── Head   ← tracked by camera / HMD                │
    /// │    ├── LeftHand  ← tracked by controller / hand         │
    /// │    └── RightHand ← tracked by controller / hand         │
    /// │                                                         │
    /// │  Inspector:                                              │
    /// │    _headTransform   → Head                              │
    /// │    _leftHandTransform  → LeftHand                       │
    /// │    _rightHandTransform → RightHand                      │
    /// │                                                         │
    /// │  AR client: _headTransform = AR Camera                  │
    /// │  VR client: _headTransform = HMD, hands = controllers   │
    /// └─────────────────────────────────────────────────────────┘
    ///
    /// How it works:
    ///   Owner (local player):
    ///     Every tick, reads tracked transforms (camera/controllers),
    ///     converts world → relative, writes to NetworkVariables.
    ///
    ///   Remote (other player's avatar on my screen):
    ///     Reads NetworkVariables, converts relative → world,
    ///     applies to avatar transforms with interpolation.
    ///
    /// Result: each client sees the other player exactly where
    /// they physically stand in the real world.
    /// </summary>
    public class PlayerColocationSync : NetworkBehaviour
    {
        [Header("Local Tracking Sources (owner only)")]
        [Tooltip("The camera / HMD transform that tracks the local player's head.")]
        [SerializeField] private Transform _headTransform;

        [Tooltip("Left controller / hand tracking transform.")]
        [SerializeField] private Transform _leftHandTransform;

        [Tooltip("Right controller / hand tracking transform.")]
        [SerializeField] private Transform _rightHandTransform;

        [Header("Remote Avatar Targets")]
        [Tooltip("Avatar head mesh/model to position for the remote player. " +
                 "If null, uses the same references as tracking sources.")]
        [SerializeField] private Transform _headAvatar;
        [SerializeField] private Transform _leftHandAvatar;
        [SerializeField] private Transform _rightHandAvatar;

        [Header("Sync Settings")]
        [SerializeField] private float _syncRate = 20f;
        [SerializeField] private float _positionThreshold = 0.001f;
        [SerializeField] private float _rotationThreshold = 0.5f;
        [SerializeField] private float _interpolationSpeed = 15f;

        // ── Network Variables (relative space) ─────────────────

        private readonly NetworkVariable<PoseData> _headPose = new(
            writePerm: NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<PoseData> _leftHandPose = new(
            writePerm: NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<PoseData> _rightHandPose = new(
            writePerm: NetworkVariableWritePermission.Owner);

        private IColocationService _service;
        private float _nextSyncTime;

        // Interpolation targets for remote
        private PoseData _headTarget, _leftTarget, _rightTarget;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Owner: auto-resolve tracking sources from the scene if not set in prefab.
            // These live on the XR Rig in the scene, not inside this prefab,
            // so they can't be assigned via Inspector on a prefab asset.
            if (IsOwner)
                ResolveTrackingSources();

            // Default avatar targets to tracking sources if not set
            if (_headAvatar == null) _headAvatar = _headTransform;
            if (_leftHandAvatar == null) _leftHandAvatar = _leftHandTransform;
            if (_rightHandAvatar == null) _rightHandAvatar = _rightHandTransform;

            // Hide remote avatar until calibration is done
            if (!IsOwner)
                SetAvatarVisibility(false);

            if (ColocationService.IsReady)
            {
                InitializeSync();
            }
            else
            {
                Debug.Log("[PlayerColocationSync] Waiting for colocation calibration...");
                StartCoroutine(WaitForService());
            }
        }

        /// <summary>
        /// Auto-resolves tracking sources from the scene's XR Rig.
        /// Only runs on the owner — remote clients don't need local tracking.
        /// </summary>
        private void ResolveTrackingSources()
        {
            if (_headTransform == null)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    _headTransform = cam.transform;
                    Debug.Log($"[PlayerColocationSync] Head auto-resolved to '{cam.name}'");
                }
                else
                {
                    Debug.LogWarning("[PlayerColocationSync] Camera.main is null — head tracking won't work.");
                }
            }

            // Hands: try to find XR controllers in the scene if not assigned.
            // This works with XRI's XRBaseController or any TrackedPoseDriver setup.
            // Leave null if not found — the sync gracefully skips null transforms.
            if (_leftHandTransform == null || _rightHandTransform == null)
            {
                var controllers = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.XRBaseController>(
                    FindObjectsSortMode.None);

                foreach (var ctrl in controllers)
                {
                    string name = ctrl.gameObject.name.ToLower();
                    if (_leftHandTransform == null && (name.Contains("left")))
                    {
                        _leftHandTransform = ctrl.transform;
                        Debug.Log($"[PlayerColocationSync] Left hand auto-resolved to '{ctrl.name}'");
                    }
                    else if (_rightHandTransform == null && (name.Contains("right")))
                    {
                        _rightHandTransform = ctrl.transform;
                        Debug.Log($"[PlayerColocationSync] Right hand auto-resolved to '{ctrl.name}'");
                    }
                }
            }
        }

        private System.Collections.IEnumerator WaitForService()
        {
            while (!ColocationService.IsReady)
                yield return null;

            Debug.Log("[PlayerColocationSync] ColocationService now ready — initializing sync.");
            InitializeSync();
        }

        private void InitializeSync()
        {
            _service = ColocationService.Instance;

            if (IsOwner)
            {
                PushAllPoses();
                SetAvatarVisibility(false);
            }
            else
            {
                ApplyRemotePose(_headAvatar, _headPose.Value);
                ApplyRemotePose(_leftHandAvatar, _leftHandPose.Value);
                ApplyRemotePose(_rightHandAvatar, _rightHandPose.Value);

                _headTarget = _headPose.Value;
                _leftTarget = _leftHandPose.Value;
                _rightTarget = _rightHandPose.Value;

                _headPose.OnValueChanged += (_, v) => _headTarget = v;
                _leftHandPose.OnValueChanged += (_, v) => _leftTarget = v;
                _rightHandPose.OnValueChanged += (_, v) => _rightTarget = v;

                SetAvatarVisibility(true);
            }
        }

        private void Update()
        {
            if (_service == null || !_service.IsReady) return;

            if (IsOwner)
                OwnerUpdate();
            else
                RemoteUpdate();
        }

        // ── Owner: track → convert → send ─────────────────────

        private void OwnerUpdate()
        {
            if (Time.time < _nextSyncTime) return;
            _nextSyncTime = Time.time + (1f / _syncRate);

            PushPoseIfChanged(_headTransform, _headPose);
            PushPoseIfChanged(_leftHandTransform, _leftHandPose);
            PushPoseIfChanged(_rightHandTransform, _rightHandPose);
        }

        private void PushPoseIfChanged(Transform source, NetworkVariable<PoseData> netVar)
        {
            if (source == null) return;

            var (relPos, relRot) = _service.WorldToRelativePose(source.position, source.rotation);
            var current = netVar.Value;

            bool posChanged = Vector3.Distance(relPos, current.Position) > _positionThreshold;
            bool rotChanged = Quaternion.Angle(relRot, current.Rotation) > _rotationThreshold;

            if (posChanged || rotChanged)    
            {
                netVar.Value = new PoseData
                {
                    Position = relPos,
                    Rotation = relRot
                };
            }
        }

        private void PushAllPoses()
        {
            if (_headTransform != null)
            {
                var (p, r) = _service.WorldToRelativePose(_headTransform.position, _headTransform.rotation);
                _headPose.Value = new PoseData { Position = p, Rotation = r };
            }
            if (_leftHandTransform != null)
            {
                var (p, r) = _service.WorldToRelativePose(_leftHandTransform.position, _leftHandTransform.rotation);
                _leftHandPose.Value = new PoseData { Position = p, Rotation = r };
            }
            if (_rightHandTransform != null)
            {
                var (p, r) = _service.WorldToRelativePose(_rightHandTransform.position, _rightHandTransform.rotation);
                _rightHandPose.Value = new PoseData { Position = p, Rotation = r };
            }
        }

        // ── Remote: receive → convert → interpolate ────────────

        private void RemoteUpdate()
        {
            InterpolatePose(_headAvatar, _headTarget);
            InterpolatePose(_leftHandAvatar, _leftTarget);
            InterpolatePose(_rightHandAvatar, _rightTarget);
        }

        private void InterpolatePose(Transform target, PoseData relativePose)
        {
            if (target == null) return;

            var (worldPos, worldRot) = _service.RelativeToWorldPose(
                relativePose.Position, relativePose.Rotation);

            float t = Time.deltaTime * _interpolationSpeed;
            target.position = Vector3.Lerp(target.position, worldPos, t);
            target.rotation = Quaternion.Slerp(target.rotation, worldRot, t);
        }

        private void ApplyRemotePose(Transform target, PoseData relativePose)
        {
            if (target == null) return;

            var (worldPos, worldRot) = _service.RelativeToWorldPose(
                relativePose.Position, relativePose.Rotation);

            target.position = worldPos;
            target.rotation = worldRot;
        }

        // ── Visibility ─────────────────────────────────────────

        private void SetAvatarVisibility(bool visible)
        {
            SetRenderers(_headAvatar, visible);
            SetRenderers(_leftHandAvatar, visible);
            SetRenderers(_rightHandAvatar, visible);
        }

        private void SetRenderers(Transform root, bool visible)
        {
            if (root == null) return;
            foreach (var r in root.GetComponentsInChildren<Renderer>())
                r.enabled = visible;
        }
    }

    /// <summary>
    /// Lightweight serializable pose for NetworkVariables.
    /// Stored in colocation-relative space (not world space).
    /// </summary>
    public struct PoseData : INetworkSerializable
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
        }
    }
}