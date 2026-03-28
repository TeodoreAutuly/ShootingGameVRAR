using UnityEngine;
using Colocation.Network;

namespace Colocation.Bootstrapper
{
    /// <summary>
    /// VR-side bootstrapper — signal-triggered calibration.
    /// 
    /// Waits for the AR player to complete the contact calibration.
    /// When the network signal arrives, captures the VR HMD's current pose
    /// as the colocation origin.
    /// 
    /// Because the phone was physically pressed against the headset at
    /// capture time, both devices share the same position. The AR side
    /// flips its rotation 180° on Y to match the HMD's forward direction,
    /// so both reference frames agree without any normalization here.
    /// 
    /// Setup:
    ///   - Attach to a GO in the VR player's scene
    ///   - Assign _hmdTransform to the VR camera / HMD transform
    ///   - Ensure a NetworkObject with ColocationSignal is spawned
    /// </summary>
    public class VRColocationBootstrapper : ColocationBootstrapperBase
    {
        [Header("VR HMD Reference")]
        [Tooltip("The VR camera / HMD transform. Its pose at signal time becomes the origin. " +
                 "If null, falls back to Camera.main.")]
        [SerializeField] private Transform _hmdTransform;

        [Header("Anchor Prefab (optional)")]
        [Tooltip("Debug prefab instantiated at the HMD pose. Deactivated after init.")]
        [SerializeField] private GameObject _anchorPrefab;

        private GameObject _anchorInstance;
        private bool _calibrated;

        protected override void Start()
        {
            base.Start();

            // Resolve HMD transform
            if (_hmdTransform == null)
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    _hmdTransform = mainCam.transform;
                    Log($"HMD transform auto-resolved to Camera.main '{mainCam.name}'");
                }
                else
                {
                    Debug.LogError("[VRColocationBootstrapper] No HMD transform assigned and Camera.main is null.");
                    return;
                }
            }

            ColocationSignal.OnCalibrationRequested += OnCalibrationSignalReceived;

            Log("Waiting for AR player to complete contact calibration...");
        }

        private void OnCalibrationSignalReceived()
        {
            if (_calibrated)
            {
                Log("Already calibrated. Ignoring duplicate signal.");
                return;
            }
            _calibrated = true;

            Log("Calibration signal received from AR player!");

            // Capture HMD pose at this instant
            // The AR side already flipped its rotation to match our forward,
            // so we use our raw pose directly — no flip, no normalization.
            Vector3 hmdPos = _hmdTransform.position;
            Quaternion hmdRot = _hmdTransform.rotation;

            Log($"HMD pose captured: pos={hmdPos:F3} rot={hmdRot.eulerAngles:F1}");

            // Instantiate anchor prefab
            if (_anchorPrefab != null)
            {
                _anchorInstance = Instantiate(_anchorPrefab, hmdPos, hmdRot);
                _anchorInstance.name = "[VR Colocation Anchor]";
                Log("Anchor prefab instantiated.");
            }

            // Commit to colocation service
            CommitAnchorPose(hmdPos, hmdRot, "vr-hmd-anchor");

            // Deactivate anchor instance
            if (_anchorInstance != null && _config != null && _config.deactivateAnchorOnReady)
            {
                if (_config.deactivationDelay > 0f)
                    StartCoroutine(DeactivateDelayed(_config.deactivationDelay));
                else
                    _anchorInstance.SetActive(false);
            }
        }

        private System.Collections.IEnumerator DeactivateDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_anchorInstance != null)
            {
                _anchorInstance.SetActive(false);
                Log("Anchor instance deactivated.");
            }
        }

        private void OnDestroy()
        {
            ColocationSignal.OnCalibrationRequested -= OnCalibrationSignalReceived;
        }
    }
}