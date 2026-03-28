using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Colocation.Config;

namespace Colocation.View
{
    /// <summary>
    /// Canvas UI that shows colocation status.
    /// Red = not ready, Green = anchored and active.
    /// Observes ColocationViewModel — zero coupling to service or network.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class ColocationCanvasView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _statusIndicator;
        [SerializeField] private TMP_Text _statusLabel;
        [SerializeField] private TMP_Text _detailLabel;

        private ColocationViewModel _viewModel;
        private ColocationConfig _config;

        // Lerp state
        private Color _colorFrom, _colorTo;
        private float _elapsed, _duration;
        private bool _transitioning;

        public void Bind(ColocationViewModel viewModel, ColocationConfig config)
        {
            _viewModel = viewModel;
            _config = config;

            Apply(_viewModel.CurrentState, immediate: true);
            _viewModel.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(ColocationViewState state)
        {
            Apply(state, immediate: false);
        }

        private void Apply(ColocationViewState state, bool immediate)
        {
            if (_statusIndicator != null)
            {
                if (immediate || _config.colorTransitionDuration <= 0f)
                {
                    _statusIndicator.color = state.StatusColor;
                    _transitioning = false;
                }
                else
                {
                    _colorFrom = _statusIndicator.color;
                    _colorTo = state.StatusColor;
                    _elapsed = 0f;
                    _duration = _config.colorTransitionDuration;
                    _transitioning = true;
                }
            }

            if (_statusLabel != null)
                _statusLabel.text = state.StatusLabel;

            if (_detailLabel != null)
                _detailLabel.text = state.IsReady
                    ? $"Anchor: {state.AnchorId}\nOrigin: {state.OriginPosition:F2}"
                    : "Waiting for anchor detection...";
        }

        private void Update()
        {
            if (!_transitioning || _statusIndicator == null) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            _statusIndicator.color = Color.Lerp(_colorFrom, _colorTo, t);

            if (t >= 1f) _transitioning = false;
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
                _viewModel.OnStateChanged -= OnStateChanged;
        }
    }
}