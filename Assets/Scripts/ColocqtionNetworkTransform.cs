using Unity.Netcode;
using UnityEngine;
using Colocation.Core;

namespace Colocation.Network
{
    /// <summary>
    /// Drop-in replacement for NetworkTransform that syncs positions
    /// in colocation-relative space instead of raw world-space.
    /// 
    /// How it works:
    ///   Owner:  world pos → WorldToRelative → write to NetworkVariable
    ///   Remote: read NetworkVariable → RelativeToWorld → apply to transform
    /// 
    /// Both clients see the object at the same physical location even though
    /// their Unity world origins differ, because both anchored on the same image.
    /// 
    /// Requires ColocationService to be initialized before this object spawns.
    /// </summary>
    public class ColocationNetworkTransform : NetworkBehaviour
    {
        [Header("Sync Settings")]
        [SerializeField] private float _syncRate = 20f; // Hz
        [SerializeField] private float _positionThreshold = 0.001f; // meters
        [SerializeField] private float _rotationThreshold = 0.5f;   // degrees

        private readonly NetworkVariable<Vector3> _relativePosition = new(
            writePerm: NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<Quaternion> _relativeRotation = new(
            writePerm: NetworkVariableWritePermission.Owner);

        private IColocationService _service;
        private float _nextSyncTime;

        // Interpolation for remotes
        private Vector3 _targetPos;
        private Quaternion _targetRot;

        [SerializeField] private float _interpolationSpeed = 15f;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (ColocationService.IsReady)
            {
                InitializeSync();
            }
            else
            {
                Debug.Log($"[ColocationNetworkTransform] '{gameObject.name}' waiting for calibration...");
                StartCoroutine(WaitForService());
            }
        }

        private System.Collections.IEnumerator WaitForService()
        {
            while (!ColocationService.IsReady)
                yield return null;

            Debug.Log($"[ColocationNetworkTransform] '{gameObject.name}' — service ready, initializing.");
            InitializeSync();
        }

        private void InitializeSync()
        {
            _service = ColocationService.Instance;

            if (IsOwner)
            {
                var (rPos, rRot) = _service.WorldToRelativePose(transform.position, transform.rotation);
                _relativePosition.Value = rPos;
                _relativeRotation.Value = rRot;
            }
            else
            {
                ApplyRelativePose(_relativePosition.Value, _relativeRotation.Value);

                _relativePosition.OnValueChanged += OnRemotePoseChanged;
                _relativeRotation.OnValueChanged += OnRemotePoseChanged;
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

        // ── Owner: write world→relative to network vars ───────

        private void OwnerUpdate()
        {
            if (Time.time < _nextSyncTime) return;
            _nextSyncTime = Time.time + (1f / _syncRate);

            var (rPos, rRot) = _service.WorldToRelativePose(transform.position, transform.rotation);

            bool posChanged = Vector3.Distance(rPos, _relativePosition.Value) > _positionThreshold;
            bool rotChanged = Quaternion.Angle(rRot, _relativeRotation.Value) > _rotationThreshold;

            if (posChanged) _relativePosition.Value = rPos;
            if (rotChanged) _relativeRotation.Value = rRot;
        }

        // ── Remote: read relative from network, apply as world ─

        private void OnRemotePoseChanged(Vector3 prev, Vector3 curr) => UpdateTarget();
        private void OnRemotePoseChanged(Quaternion prev, Quaternion curr) => UpdateTarget();

        private void UpdateTarget()
        {
            var (wPos, wRot) = _service.RelativeToWorldPose(
                _relativePosition.Value, _relativeRotation.Value);
            _targetPos = wPos;
            _targetRot = wRot;
        }

        private void RemoteUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, _targetPos,
                Time.deltaTime * _interpolationSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot,
                Time.deltaTime * _interpolationSpeed);
        }

        private void ApplyRelativePose(Vector3 relPos, Quaternion relRot)
        {
            var (wPos, wRot) = _service.RelativeToWorldPose(relPos, relRot);
            transform.position = wPos;
            transform.rotation = wRot;
            _targetPos = wPos;
            _targetRot = wRot;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
            {
                _relativePosition.OnValueChanged -= OnRemotePoseChanged;
                _relativeRotation.OnValueChanged -= OnRemotePoseChanged;
            }
            base.OnNetworkDespawn();
        }
    }
}