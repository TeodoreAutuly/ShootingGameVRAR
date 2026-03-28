using System;
using UnityEngine;
using Colocation.Core;
using Colocation.Config;

namespace Colocation.View
{
    /// <summary>
    /// ViewModel layer — transforms service state into display-ready data.
    /// The View observes this, never touches the service.
    /// </summary>
    public class ColocationViewModel
    {
        public event Action<ColocationViewState> OnStateChanged;

        private readonly ColocationConfig _config;
        private ColocationViewState _current;

        public ColocationViewState CurrentState => _current;

        public ColocationViewModel(ColocationConfig config)
        {
            _config = config;
            _current = new ColocationViewState
            {
                IsReady = false,
                StatusColor = _config.colorNotReady,
                StatusLabel = "Colocation: Waiting for anchor...",
                AnchorId = "—",
                OriginPosition = Vector3.zero,
            };
        }

        public void NotifyServiceReady(IColocationService service)
        {
            _current = new ColocationViewState
            {
                IsReady = true,
                StatusColor = _config.colorReady,
                StatusLabel = "Colocation: Active",
                AnchorId = service.AnchorId,
                OriginPosition = service.OriginPosition,
            };
            OnStateChanged?.Invoke(_current);
        }

        public void NotifyOriginUpdated(IColocationService service)
        {
            _current = new ColocationViewState
            {
                IsReady = true,
                StatusColor = _config.colorReady,
                StatusLabel = "Colocation: Active (re-anchored)",
                AnchorId = service.AnchorId,
                OriginPosition = service.OriginPosition,
            };
            OnStateChanged?.Invoke(_current);
        }
    }

    public struct ColocationViewState
    {
        public bool IsReady;
        public Color StatusColor;
        public string StatusLabel;
        public string AnchorId;
        public Vector3 OriginPosition;
    }
}