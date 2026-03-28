using System;
using UnityEngine;

namespace Colocation.Core
{
    /// <summary>
    /// Local singleton service — the single access point for all colocation math.
    /// 
    /// Initialized by a platform-specific bootstrapper (AR or VR).
    /// Consumers use it to convert coordinates before/after NGO sync:
    /// 
    ///   // Sending:
    ///   var rel = ColocationService.Instance.WorldToRelative(transform.position);
    ///   myNetworkVar.Value = rel;
    /// 
    ///   // Receiving:
    ///   transform.position = ColocationService.Instance.RelativeToWorld(networkVar.Value);
    /// </summary>
    public class ColocationService : MonoSingleton<ColocationService>, IColocationService
    {
        public event Action OnServiceReady;
        public event Action OnOriginUpdated;

        private ColocationModel _model;

        bool IColocationService.IsReady => IsReady;
        public string AnchorId => _model?.AnchorId;
        public Vector3 OriginPosition => _model?.OriginPosition ?? Vector3.zero;
        public Quaternion OriginRotation => _model?.OriginRotation ?? Quaternion.identity;

        // ── Initialization (called once by bootstrapper) ───────

        public void Initialize(ColocationModel model)
        {
            if (IsReady)
            {
                Debug.LogWarning("[ColocationService] Already initialized.");
                return;
            }

            _model = model ?? throw new ArgumentNullException(nameof(model));
            _model.OnOriginUpdated += () => OnOriginUpdated?.Invoke();

            MarkInitialized();

            Debug.Log($"[ColocationService] Ready — anchor '{_model.AnchorId}' " +
                      $"at pos={_model.OriginPosition:F3} rot={_model.OriginRotation.eulerAngles:F1}");

            OnServiceReady?.Invoke();
        }

        // ── IColocationService ─────────────────────────────────

        public Vector3 WorldToRelative(Vector3 worldPos)             { AssertReady(); return _model.WorldToRelative(worldPos); }
        public Vector3 RelativeToWorld(Vector3 relPos)               { AssertReady(); return _model.RelativeToWorld(relPos); }
        public Quaternion WorldToRelativeRotation(Quaternion wRot)    { AssertReady(); return _model.WorldToRelativeRotation(wRot); }
        public Quaternion RelativeToWorldRotation(Quaternion rRot)    { AssertReady(); return _model.RelativeToWorldRotation(rRot); }

        public (Vector3 pos, Quaternion rot) WorldToRelativePose(Vector3 wPos, Quaternion wRot)
        { AssertReady(); return _model.WorldToRelativePose(wPos, wRot); }

        public (Vector3 pos, Quaternion rot) RelativeToWorldPose(Vector3 rPos, Quaternion rRot)
        { AssertReady(); return _model.RelativeToWorldPose(rPos, rRot); }

        public float DistanceFromOrigin(Vector3 worldPos)            { AssertReady(); return _model.DistanceFromOrigin(worldPos); }
        public Vector3 DirectionFromOrigin(Vector3 worldPos)         { AssertReady(); return _model.DirectionFromOrigin(worldPos); }

        public void UpdateOrigin(Vector3 newPos, Quaternion newRot)
        {
            AssertReady();
            _model.SetOrigin(newPos, newRot);
            Debug.Log($"[ColocationService] Origin re-anchored to {newPos:F3}");
        }

        // ── Guard ──────────────────────────────────────────────

        private void AssertReady()
        {
            if (!IsReady)
                throw new InvalidOperationException(
                    "[ColocationService] Not initialized. Anchor the image/controller first.");
        }

        protected override void OnDestroy()
        {
            OnServiceReady = null;
            OnOriginUpdated = null;
            base.OnDestroy();
        }
    }
}