using UnityEngine;
using Colocation.Core;
using Colocation.Config;
using Colocation.View;

namespace Colocation.Bootstrapper
{
    /// <summary>
    /// Base bootstrapper — contains the shared init pipeline.
    /// Platform-specific subclasses (AR / VR) only differ in HOW they capture the anchor pose.
    /// 
    /// Lifecycle:
    ///   1. Subclass detects anchor (tracked image / controller press)
    ///   2. Subclass calls CommitAnchorPose(position, rotation)
    ///   3. Base creates Model → initializes Service → binds ViewModel → deactivates GO
    /// </summary>
    public abstract class ColocationBootstrapperBase : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] protected ColocationConfig _config;

        [Header("View (optional)")]
        [SerializeField] private ColocationCanvasView _canvasView;

        private ColocationViewModel _viewModel;
        private bool _committed;

        protected virtual void Start()
        {
            if (_config == null)
            {
                Debug.LogError("[ColocationBootstrapper] Config is null. Assign a ColocationConfig asset.");
                return;
            }

            // Pre-init the ViewModel so the view shows "not ready" immediately
            _viewModel = new ColocationViewModel(_config);

            if (_canvasView != null)
                _canvasView.Bind(_viewModel, _config);

            Log("Bootstrapper started. Waiting for anchor pose...");
        }

        /// <summary>
        /// Called by the platform-specific subclass once the anchor pose is captured.
        /// This is the single entry point into the shared init pipeline.
        /// </summary>
        protected void CommitAnchorPose(Vector3 position, Quaternion rotation, string anchorId = null)
        {
            if (_committed)
            {
                Log("Anchor already committed. Ignoring.");
                return;
            }
            _committed = true;

            // 1. Create model from captured pose
            var model = new ColocationModel(position, rotation, anchorId);

            // 2. Ensure service GO exists and initialize
            var serviceGo = new GameObject("[ColocationService]");
            var service = serviceGo.AddComponent<ColocationService>();
            service.Initialize(model);

            // 3. Notify ViewModel → View updates
            _viewModel?.NotifyServiceReady(service);

            // 4. Subscribe to future origin updates for the view
            service.OnOriginUpdated += () => _viewModel?.NotifyOriginUpdated(service);

            Log($"Anchor committed: pos={position:F3} rot={rotation.eulerAngles:F1} id={model.AnchorId}");

            // 5. Deactivate the anchor GO (it served its purpose)
            if (_config.deactivateAnchorOnReady)
            {
                if (_config.deactivationDelay > 0f)
                    Invoke(nameof(DeactivateAnchor), _config.deactivationDelay);
                else
                    DeactivateAnchor();
            }
        }

        private void DeactivateAnchor()
        {
            Log("Anchor GO not deactivated.");
            //gameObject.SetActive(false);
        }

        protected void Log(string msg)
        {
            if (_config != null && _config.verboseLogging)
                Debug.Log($"[ColocationBootstrapper:{GetType().Name}] {msg}");
        }
    }
}