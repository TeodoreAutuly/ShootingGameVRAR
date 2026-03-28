using System;
using UnityEngine;

namespace Colocation.Core
{
    /// <summary>
    /// Pure data model for a colocation anchor.
    /// Stores the world-space pose of the local physical anchor (tracked image / controller)
    /// and provides bidirectional conversion between local world-space and shared relative space.
    ///
    /// Relative space is the common language:
    ///   AR client: WorldToRelative(myWorldPos) → send via NGO
    ///   VR client: receives relativePos → RelativeToWorld(relativePos)
    ///   Both see the object at the same physical location.
    /// </summary>
    [Serializable]
    public class ColocationModel
    {
        public event Action OnOriginUpdated;

        [SerializeField] private Vector3 _originPosition;
        [SerializeField] private Quaternion _originRotation;
        [SerializeField] private string _anchorId;
        [SerializeField] private double _timestampUtc;

        // Cache the inverse rotation — used on every conversion
        private Quaternion _inverseRotation;

        public Vector3 OriginPosition => _originPosition;
        public Quaternion OriginRotation => _originRotation;
        public string AnchorId => _anchorId;
        public double TimestampUtc => _timestampUtc;

        public ColocationModel(Vector3 position, Quaternion rotation, string anchorId = null)
        {
            _originPosition = position;
            _originRotation = rotation;
            _inverseRotation = Quaternion.Inverse(rotation);
            _anchorId = anchorId ?? Guid.NewGuid().ToString("N")[..8];
            _timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public void SetOrigin(Vector3 position, Quaternion rotation)
        {
            _originPosition = position;
            _originRotation = rotation;
            _inverseRotation = Quaternion.Inverse(rotation);
            _timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            OnOriginUpdated?.Invoke();
        }

        // ── World ↔ Relative ───────────────────────────────────

        public Vector3 WorldToRelative(Vector3 worldPos)
            => _inverseRotation * (worldPos - _originPosition);

        public Vector3 RelativeToWorld(Vector3 relPos)
            => _originPosition + (_originRotation * relPos);

        public Quaternion WorldToRelativeRotation(Quaternion worldRot)
            => _inverseRotation * worldRot;

        public Quaternion RelativeToWorldRotation(Quaternion relRot)
            => _originRotation * relRot;

        public (Vector3 pos, Quaternion rot) WorldToRelativePose(Vector3 wPos, Quaternion wRot)
            => (WorldToRelative(wPos), WorldToRelativeRotation(wRot));

        public (Vector3 pos, Quaternion rot) RelativeToWorldPose(Vector3 rPos, Quaternion rRot)
            => (RelativeToWorld(rPos), RelativeToWorldRotation(rRot));

        // ── Spatial helpers ────────────────────────────────────

        public float DistanceFromOrigin(Vector3 worldPos)
            => Vector3.Distance(_originPosition, worldPos);

        public Vector3 DirectionFromOrigin(Vector3 worldPos)
            => (worldPos - _originPosition).normalized;
    }
}