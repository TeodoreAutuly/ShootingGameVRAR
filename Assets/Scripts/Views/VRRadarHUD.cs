using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barre radar horizontale affichée dans le HUD VR.
///
/// Affiche un indicateur visuel par cible active, positionné horizontalement
/// selon l'angle signé entre la direction du regard et la direction vers la cible.
///
/// Setup Unity (dans le prefab Player_VR, sous HUD Canvas) :
///   HUD Canvas
///   └── RadarBar         [RectTransform — ancre haut-centre]
///       └── Indicator    [RectTransform + Image — template désactivé]
///
/// Setup Inspector :
///   - headTransform      : Transform de la caméra principale (tête VR)
///   - radarBar           : RectTransform de la barre (largeur = FOV visuel)
///   - indicatorTemplate  : RectTransform du template de point (désactivé par défaut)
///   - horizontalFOV      : angle couvert par la largeur totale de la barre (défaut 90°)
///   - maxIndicators      : taille du pool (doit être ≥ nombre max de cibles actives)
///
/// Le radar n'est visible que sur la machine propriétaire (IsOwner).
/// </summary>
public class VRRadarHUD : NetworkBehaviour
{
    [Header("Références")]
    [Tooltip("Transform de la caméra VR principale (tête).")]
    [SerializeField] private Transform headTransform;

    [Tooltip("RectTransform de la barre radar (parent des indicateurs).")]
    [SerializeField] private RectTransform radarBar;

    [Tooltip("Template d'indicateur — doit être désactivé dans le prefab.")]
    [SerializeField] private RectTransform indicatorTemplate;

    [Header("Paramètres")]
    [Tooltip("FOV horizontal couvert par la barre complète (°). Ex: 90 → ±45° autour du regard.")]
    [SerializeField] private float horizontalFOV = 90f;

    [Tooltip("Nombre maximum d'indicateurs dans le pool (≥ nombre de cibles max).")]
    [SerializeField] private int maxIndicators = 8;

    // ── Pool ──────────────────────────────────────────────────────────────────
    private readonly List<RectTransform> _pool = new List<RectTransform>();

    // ─────────────────────────────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(IsOwner);

        if (!IsOwner) return;

        // Auto-découverte de la caméra tête si non assignée
        if (headTransform == null)
        {
            Camera cam = GetComponentInParent<Camera>();
            if (cam != null) headTransform = cam.transform;
        }

        BuildPool();
    }

    private void BuildPool()
    {
        if (indicatorTemplate == null || radarBar == null) return;

        indicatorTemplate.gameObject.SetActive(false);

        for (int i = 0; i < maxIndicators; i++)
        {
            RectTransform clone = Instantiate(indicatorTemplate, radarBar);
            clone.gameObject.SetActive(false);
            _pool.Add(clone);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    private void LateUpdate()
    {
        if (!IsOwner || TargetManager.Instance == null) return;

        // Remettre tous les indicateurs dans le pool
        foreach (RectTransform ind in _pool)
            ind.gameObject.SetActive(false);

        if (headTransform == null) return;

        IReadOnlyList<TargetController> active = TargetManager.Instance.GetActiveTargets();

        float barHalfWidth = radarBar != null ? radarBar.rect.width * 0.5f : 200f;
        float halfFOV      = horizontalFOV * 0.5f;

        for (int i = 0; i < active.Count && i < _pool.Count; i++)
        {
            TargetController target = active[i];
            if (target == null) continue;

            // Direction horizontale vers la cible
            Vector3 toTarget = target.transform.position - headTransform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.001f) continue;

            // Avant horizontal de la tête
            Vector3 forward = headTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) continue;

            // Angle signé (gauche = négatif, droite = positif)
            float signedAngle = Vector3.SignedAngle(forward, toTarget, Vector3.up);

            // Normalisation [-1, 1] clampée au FOV
            float normalized = Mathf.Clamp(signedAngle / halfFOV, -1f, 1f);
            float xPos       = normalized * barHalfWidth;

            RectTransform indicator = _pool[i];
            indicator.anchoredPosition = new Vector2(xPos, 0f);
            indicator.gameObject.SetActive(true);
        }
    }
}
