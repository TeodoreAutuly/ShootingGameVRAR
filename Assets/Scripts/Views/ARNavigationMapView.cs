using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VIEW — Affiche la flèche de direction et les points rouges des targets sur le canvas.
///
/// Setup Unity :
///   - Ce script est sur le GO racine du board AR.
///   - _arrow        : Transform de la flèche (enfant du board).
///   - _dotsContainer: RectTransform parent dans lequel les dots sont instanciés.
///   - _dotPrefab    : prefab UI simple — Image ronde rouge, pivot centré, taille ~20×20.
/// </summary>
public class ARNavigationMapView : MonoBehaviour
{
    [Header("Flèche")]
    [SerializeField] private Transform _arrow;

    [Header("Dots targets")]
    [SerializeField] private RectTransform _dotsContainer;
    [SerializeField] private GameObject    _dotPrefab;

    // Pool de dots instanciés
    private readonly List<RectTransform> _dots = new();

    // ── Flèche ────────────────────────────────────────────────────────────────

    public void UpdateArrow(float rotationY)
    {
        if (_arrow == null) return;

        _arrow.localRotation = Quaternion.Euler(
            _arrow.localEulerAngles.x,
            rotationY,
            _arrow.localEulerAngles.z
        );
    }

    // ── Dots targets ──────────────────────────────────────────────────────────

    /// Met à jour les points rouges sur le canvas.
    /// canvasPositions : positions en unités RectTransform (0,0 = centre du canvas).
    public void UpdateDots(IReadOnlyList<Vector2> canvasPositions)
    {
        if (_dotsContainer == null || _dotPrefab == null) return;

        // Ajuste le nombre de dots au nombre de targets
        AdjustDotCount(canvasPositions.Count);

        // Positionne chaque dot
        for (int i = 0; i < canvasPositions.Count; i++){
            _dots[i].anchoredPosition = canvasPositions[i];
            Debug.Log($"Dot OK...{i}");
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void AdjustDotCount(int targetCount)
    {
        // Instancie les dots manquants
        while (_dots.Count < targetCount)
        {
            GameObject go = Instantiate(_dotPrefab, _dotsContainer);
            _dots.Add(go.GetComponent<RectTransform>());
        }

        // Désactive les dots en surplus
        for (int i = 0; i < _dots.Count; i++)
            _dots[i].gameObject.SetActive(i < targetCount);
    }
}