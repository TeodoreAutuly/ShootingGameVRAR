using System;
using UnityEngine;

namespace Colocation.Core
{
    /// <summary>
    /// Public contract for the colocation service.
    /// Consumers (NetworkTransform wrappers, gameplay scripts) depend on this,
    /// not on the concrete singleton or platform-specific bootstrappers.
    /// 
    /// The service converts between each client's local world-space and
    /// the shared relative space. Two clients that anchored on the same
    /// physical image will agree on relative coordinates even though their
    /// Unity world-space origins differ.
    /// </summary>
    public interface IColocationService
    {
        event Action OnServiceReady;
        event Action OnOriginUpdated;

        bool IsReady { get; }
        string AnchorId { get; }
        Vector3 OriginPosition { get; }
        Quaternion OriginRotation { get; }

        // ── Core conversions (the whole point) ─────────────────
        // WorldToRelative: call BEFORE sending over NGO
        // RelativeToWorld: call AFTER receiving from NGO

        Vector3 WorldToRelative(Vector3 worldPosition);
        Vector3 RelativeToWorld(Vector3 relativePosition);
        Quaternion WorldToRelativeRotation(Quaternion worldRotation);
        Quaternion RelativeToWorldRotation(Quaternion relativeRotation);

        (Vector3 pos, Quaternion rot) WorldToRelativePose(Vector3 worldPos, Quaternion worldRot);
        (Vector3 pos, Quaternion rot) RelativeToWorldPose(Vector3 relPos, Quaternion relRot);

        // ── Spatial helpers ────────────────────────────────────

        float DistanceFromOrigin(Vector3 worldPosition);
        Vector3 DirectionFromOrigin(Vector3 worldPosition);

        /// <summary>Re-anchor the origin (use sparingly — shifts all relative coords).</summary>
        void UpdateOrigin(Vector3 newPosition, Quaternion newRotation);
    }
}