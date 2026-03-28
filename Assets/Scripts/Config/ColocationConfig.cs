using UnityEngine;

namespace Colocation.Config
{
    /// <summary>
    /// Editor-configurable parameters. Create via Assets → Create → Colocation → Config.
    /// </summary>
    [CreateAssetMenu(fileName = "ColocationConfig", menuName = "Colocation/Config")]
    public class ColocationConfig : ScriptableObject
    {
        [Header("Bootstrapper")]
        [Tooltip("Deactivate the anchor GO after the service is initialized.")]
        public bool deactivateAnchorOnReady = true;

        [Tooltip("Delay before deactivation (seconds).")]
        [Range(0f, 5f)]
        public float deactivationDelay = 0f;

        [Header("Calibration Countdown")]
        [Tooltip("Countdown duration (seconds) before capturing the AR camera pose.")]
        [Range(1, 10)]
        public int countdownSeconds = 3;

        [Header("Visual Feedback")]
        public Color colorNotReady = new Color(0.85f, 0.15f, 0.15f, 1f);
        public Color colorReady = new Color(0.15f, 0.85f, 0.15f, 1f);

        [Range(0f, 2f)]
        public float colorTransitionDuration = 0.4f;

        [Header("Logging")]
        public bool verboseLogging = true;
    }
}