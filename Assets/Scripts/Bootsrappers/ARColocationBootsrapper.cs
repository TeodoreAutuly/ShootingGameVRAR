using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Colocation.Network;

namespace Colocation.Bootstrapper
{
    /// <summary>
    /// AR-side bootstrapper — contact calibration with countdown.
    /// 
    /// The AR player presses a button, a countdown starts (3, 2, 1),
    /// during which they physically press their phone against the front
    /// of the VR headset. At zero, the AR camera's pose is captured
    /// as the colocation origin.
    /// 
    /// Because the phone and HMD are pressed together face-to-face,
    /// the two devices share the same physical position and their
    /// forward axes are naturally opposed (phone camera faces the HMD,
    /// HMD faces away). No rotation normalization or image tracking needed.
    /// 
    /// Setup:
    ///   - Attach to a GO in the AR scene
    ///   - Assign the AR Camera transform
    ///   - Assign UI elements (button, countdown label)
    ///   - Ensure a NetworkObject with ColocationSignal exists
    /// </summary>
    public class ARColocationBootstrapper : ColocationBootstrapperBase
    {
        [Header("AR Camera")]
        [Tooltip("The AR camera transform. Its pose at countdown zero becomes the origin. " +
                 "If null, falls back to Camera.main.")]
        [SerializeField] private Transform _arCameraTransform;

        [Header("Calibration UI")]
        [Tooltip("Button that starts the calibration countdown.")]
        [SerializeField] private Button _calibrateButton;

        [Tooltip("Label showing the countdown (3, 2, 1, GO). Hidden when idle.")]
        [SerializeField] private TMP_Text _countdownLabel;

        [Tooltip("Optional instruction text shown before calibration.")]
        [SerializeField] private TMP_Text _instructionLabel;

        [Header("Countdown Settings")]
        [Header("Anchor Prefab (optional)")]
        [SerializeField] private GameObject _anchorPrefab;

        private GameObject _anchorInstance;
        private bool _calibrated;

        private const string INSTRUCTION_IDLE =
            "Press the button, then hold your phone flat against the front of the VR headset.";
        private const string INSTRUCTION_COUNTDOWN =
            "Hold steady against the headset...";
        private const string INSTRUCTION_DONE =
            "Calibration complete!";

        protected override void Start()
        {
            base.Start();

            // Resolve AR camera
            if (_arCameraTransform == null)
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    _arCameraTransform = mainCam.transform;
                    Log($"AR camera auto-resolved to '{mainCam.name}'");
                }
                else
                {
                    Debug.LogError("[ARColocationBootstrapper] No AR camera assigned and Camera.main is null.");
                    return;
                }
            }

            // Setup UI
            if (_calibrateButton != null)
                _calibrateButton.onClick.AddListener(OnCalibratePressed);

            if (_countdownLabel != null)
                _countdownLabel.gameObject.SetActive(false);

            if (_instructionLabel != null)
                _instructionLabel.text = INSTRUCTION_IDLE;

            Log("Ready. Press calibrate button to start countdown.");
        }

        private void OnCalibratePressed()
        {
            if (_calibrated) return;

            _calibrateButton.interactable = false;

            if (_instructionLabel != null)
                _instructionLabel.text = INSTRUCTION_COUNTDOWN;

            StartCoroutine(CountdownAndCapture());
        }

        private IEnumerator CountdownAndCapture()
        {
            if (_countdownLabel != null)
                _countdownLabel.gameObject.SetActive(true);

            // Countdown
            for (int i = _config.countdownSeconds; i > 0; i--)
            {
                if (_countdownLabel != null)
                    _countdownLabel.text = i.ToString();

                Log($"Countdown: {i}");
                yield return new WaitForSeconds(1f);
            }

            // Capture
            if (_countdownLabel != null)
                _countdownLabel.text = "GO";

            _calibrated = true;

            Vector3 pos = _arCameraTransform.position;
            Quaternion rot = _arCameraTransform.rotation;

            Log($"AR camera pose captured: pos={pos:F3} rot={rot.eulerAngles:F1}");

            // The phone is pressed against the HMD face-to-face.
            // The AR camera looks INTO the headset (forward = toward HMD).
            // The VR HMD looks AWAY from the phone (forward = opposite).
            // We flip the AR forward by 180° around Y so both reference
            // frames agree on the "forward" direction.
            Quaternion flippedRot = rot * Quaternion.Euler(0f, 180f, 0f);

            Log($"Rotation flipped 180° on Y: {rot.eulerAngles:F1} → {flippedRot.eulerAngles:F1}");

            // Instantiate anchor prefab
            if (_anchorPrefab != null)
            {
                _anchorInstance = Instantiate(_anchorPrefab, pos, flippedRot);
                _anchorInstance.name = "[AR Colocation Anchor]";
            }

            // Commit to colocation service — BUT don't call it yet,
            // because the base class may deactivate this GO and kill the coroutine.
            // Finish all UI work first.

            // Notify VR client
            if (ColocationSignal.Instance != null)
            {
                ColocationSignal.Instance.SendCalibrationSignal();
                Log("Calibration signal sent to VR.");
            }
            else
            {
                Debug.LogWarning("[ARColocationBootstrapper] ColocationSignal not found. " +
                                 "VR client won't be notified.");
            }

            // Update UI
            if (_instructionLabel != null)
                _instructionLabel.text = INSTRUCTION_DONE;

            yield return new WaitForSeconds(1f);

            if (_countdownLabel != null)
                _countdownLabel.gameObject.SetActive(false);

            // Deactivate anchor instance
            if (_anchorInstance != null && _config != null && _config.deactivateAnchorOnReady)
            {
                if (_config.deactivationDelay > 0f)
                    yield return new WaitForSeconds(_config.deactivationDelay);

                _anchorInstance.SetActive(false);
                Log("Anchor instance deactivated.");
            }

            // NOW commit — this may deactivate this GO (last thing we do)
            CommitAnchorPose(pos, flippedRot, "ar-contact-anchor");
        }

        private void OnDestroy()
        {
            if (_calibrateButton != null)
                _calibrateButton.onClick.RemoveListener(OnCalibratePressed);
        }
    }
}