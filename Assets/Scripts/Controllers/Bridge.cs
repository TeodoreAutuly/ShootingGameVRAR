using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using Fr.ImtAtlantique.CEXIHA.Core;

namespace Colocation
{
    /// <summary>
    /// Bridges the existing CalibrationManager with the networked AR/VR colocation setup.
    /// 
    /// Problem: CalibrationManager needs a referencePose (the VR player's camera),
    /// but that's a NetworkObject spawned at runtime — can't assign it in the Inspector.
    /// 
    /// This script:
    ///   1. Waits for the remote VR player's head transform to appear on the network
    ///   2. Assigns it as referencePose on the CalibrationManager
    ///   3. Shows a "Calibrate" button when ready
    ///   4. On button press → countdown → player presses phone against VR headset → Calibrate()
    /// 
    /// Setup (AR scene):
    ///   - Attach to any GO in the AR scene
    ///   - Assign CalibrationManager (already configured with objectToCalibrate = XR Origin, 
    ///     poseToAlign = AR Camera)
    ///   - Assign UI elements
    ///   - referencePose is resolved automatically from the VR player's networked head
    /// 
    /// The VR player's prefab must have a child tagged "PlayerHead" or with a
    /// component you can identify — see ResolveVRHead() to adapt to your setup.
    /// </summary>
    public class ColocationCalibrationBridge : MonoBehaviour
    {
        [Header("Calibration")]
        [SerializeField] private CalibrationManager _calibrationManager;

        [Header("UI")]
        [SerializeField] private Button _calibrateButton;
        [SerializeField] private TMP_Text _countdownLabel;
        [SerializeField] private TMP_Text _instructionLabel;

        [Header("Settings")]
        [SerializeField] private int _countdownSeconds = 5;

        [Header("VR Head Resolution")]
        [Tooltip("Tag on the VR player's head child object. Used to find the referencePose at runtime.")]
        [SerializeField] private string _vrHeadTag = "PlayerHead";

        private bool _referenceFound;
        private bool _calibrating;

        private void Start()
        {
            if (_calibrateButton != null)
            {
                _calibrateButton.interactable = false;
                _calibrateButton.onClick.AddListener(OnCalibratePressed);
            }

            if (_countdownLabel != null)
                _countdownLabel.gameObject.SetActive(false);

            if (_instructionLabel != null)
                _instructionLabel.text = "Waiting for VR player to connect...";
        }

        private void Update()
        {
            if (_referenceFound || _calibrating) return;

            // Try to find the VR player's head every frame until found
            Transform vrHead = FindVRHead();
            if (vrHead != null)
            {
                _referenceFound = true;
                _calibrationManager.referencePose = vrHead;

                if (_calibrateButton != null)
                    _calibrateButton.interactable = true;

                if (_instructionLabel != null)
                    _instructionLabel.text = "VR player found!\nPress phone against VR headset, then tap Calibrate.";

                Debug.Log($"[ColocationBridge] VR head found: '{vrHead.name}' — ready to calibrate.");
            }
        }

        /// <summary>
        /// Finds the remote VR player's head transform in the scene.
        /// Adapt this method to match your player prefab structure.
        /// </summary>
        private Transform FindVRHead()
        {
            // Strategy 1: Find by tag
            GameObject tagged = GameObject.FindWithTag(_vrHeadTag);
            if (tagged != null)
            {
                // Make sure it's a remote player (not our own)
                var netObj = tagged.GetComponentInParent<NetworkObject>();
                if (netObj != null && !netObj.IsOwner)
                    return tagged.transform;
            }

            // Strategy 2: Find all network players, pick the remote one's head
            // Uncomment and adapt if you prefer finding by component:
            // 
            // var players = FindObjectsByType<PlayerColocationSync>(FindObjectsSortMode.None);
            // foreach (var player in players)
            // {
            //     if (!player.IsOwner && player.HeadAvatar != null)
            //         return player.HeadAvatar;
            // }

            return null;
        }

        private void OnCalibratePressed()
        {
            if (_calibrating || !_referenceFound) return;
            _calibrating = true;

            _calibrateButton.interactable = false;
            StartCoroutine(CountdownAndCalibrate());
        }

        private System.Collections.IEnumerator CountdownAndCalibrate()
        {
            if (_instructionLabel != null)
                _instructionLabel.text = "Hold phone against VR headset...";

            if (_countdownLabel != null)
                _countdownLabel.gameObject.SetActive(true);

            // Countdown
            for (int i = _countdownSeconds; i > 0; i--)
            {
                if (_countdownLabel != null)
                    _countdownLabel.text = i.ToString();

                yield return new WaitForSeconds(1f);
            }

            if (_countdownLabel != null)
                _countdownLabel.text = "GO";

            // Calibrate — moves the XR Origin so AR camera aligns with VR camera
            _calibrationManager.Calibrate();

            Debug.Log("[ColocationBridge] Calibration done!");

            if (_instructionLabel != null)
                _instructionLabel.text = "Calibration complete!";

            yield return new WaitForSeconds(1f);

            if (_countdownLabel != null)
                _countdownLabel.gameObject.SetActive(false);

            // Allow re-calibration if needed
            _calibrating = false;
            _calibrateButton.interactable = true;

            if (_instructionLabel != null)
                _instructionLabel.text = "Calibrated. Tap again to re-calibrate.";
        }

        private void OnDestroy()
        {
            if (_calibrateButton != null)
                _calibrateButton.onClick.RemoveListener(OnCalibratePressed);
        }
    }
}