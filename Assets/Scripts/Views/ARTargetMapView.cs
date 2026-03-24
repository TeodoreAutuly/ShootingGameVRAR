using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VIEW Screen Space — Affiche les points rouges des targets sur l'UI AR NavShow.
///
/// Setup Unity :
///   Ce script est sur le GO "UI AR NavShow" (Canvas Screen Space Overlay).
///   - _center     : RectTransform du GO "Center" (pivot 0,0 des dots).
///   - _dotPrefab  : prefab UI Image ronde rouge, pivot centré (0.5, 0.5), ~20×20px.
///   - _mapRadius  : rayon en pixels de la zone d'affichage (pour clamper les dots).
/// </summary>
public class ARTargetMapView : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private RectTransform _center;
    [SerializeField] private GameObject    _dotPrefab;

    [Header("Settings")]
    [Tooltip("Rayon max en pixels depuis le centre — dots clampés à ce rayon.")]
    [SerializeField] private float _mapRadius = 80f;

    private readonly List<RectTransform> _dots = new();

    // ── API publique ──────────────────────────────────────────────────────────

    /// Met à jour les dots à partir des positions canvas (pixels relatifs à Center).
    public void UpdateDots(IReadOnlyList<Vector2> canvasPositions)
    {
        if (_center == null || _dotPrefab == null)
        {
            Debug.LogWarning("[ARTargetMapView] _center ou _dotPrefab non assigné.", this);
            return;
        }

        AdjustDotCount(canvasPositions.Count);

        for (int i = 0; i < canvasPositions.Count; i++)
            _dots[i].anchoredPosition = canvasPositions[i];
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void AdjustDotCount(int targetCount)
    {
        if (_dotPrefab == null) { Debug.LogError("[ARTargetMapView] _dotPrefab est null — assigne le prefab dot dans l Inspector.", this); return; }
        if (_center == null)    { Debug.LogError("[ARTargetMapView] _center est null — assigne le RectTransform Center dans l Inspector.", this); return; }

        while (_dots.Count < targetCount)
        {
            GameObject go = Instantiate(_dotPrefab, _center);
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) { Debug.LogError("[ARTargetMapView] Le dotPrefab n a pas de RectTransform.", this); break; }
            rt.anchoredPosition = Vector2.zero;
            _dots.Add(rt);
            Debug.Log($"[ARTargetMapView] Dot {_dots.Count} instancie en {rt.anchoredPosition}.", this);
        }

        for (int i = 0; i < _dots.Count; i++)
            _dots[i].gameObject.SetActive(i < targetCount);

        Debug.Log($"[ARTargetMapView] {targetCount} dots actifs sur {_dots.Count} total.", this);
    }
  
    public float MapRadius => _mapRadius;
}