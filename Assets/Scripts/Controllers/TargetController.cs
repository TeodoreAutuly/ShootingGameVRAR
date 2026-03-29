using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Gère l'état d'une cible synchronisée réseau.
///
/// - AR : touche l'écran → ActivateByARServerRpc() rend la cible active.
/// - VR : tire sur une cible active → HitByVRServerRpc() la remet inactive + déclenche OnTargetHit.
/// - Cycle : inactive → active → inactive (jamais despawned).
///
/// Layers :
///   Layer 8 (Target_Inactive) — AR voit (semi-transparent), VR ne voit pas.
///   Layer 9 (Target_Active)   — Tous voient, VR peut tirer dessus.
/// </summary>
public class TargetController : NetworkBehaviour
{
    // ── Static event pour le score ─────────────────────────────────────────────
    /// <summary>Déclenché serveur-side quand un joueur VR touche une cible active.</summary>
    public static event Action<ulong> OnTargetHit;

    // ── Layer indices ──────────────────────────────────────────────────────────
    private const int LayerInactive = 8; // Target_Inactive
    private const int LayerActive   = 9; // Target_Active

    // ── Matériaux ──────────────────────────────────────────────────────────────
    [Header("Matériaux")]
    [Tooltip("Matériau quand la cible est active (rouge opaque).")]
    [SerializeField] private Material activeMaterial;

    [Tooltip("Matériau quand la cible est inactive (gris semi-transparent, alpha ~0.3).")]
    [SerializeField] private Material inactiveMaterial;

    // ── Feedback visuel ────────────────────────────────────────────────────────
    [Header("Feedback")]
    [Tooltip("Durée de la pulsation à l'activation (secondes).")]
    [SerializeField] private float activationPulseDuration = 0.4f;

    // ── État réseau ────────────────────────────────────────────────────────────
    private readonly NetworkVariable<bool> _isActive = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    /// <summary>La cible est-elle actuellement active ?</summary>
    public bool IsActive => _isActive.Value;

    // ── Références ────────────────────────────────────────────────────────────
    private Renderer              _renderer;
    private Collider              _collider;
    // MaterialPropertyBlock pré-alloué pour le pulse — évite de créer une nouvelle
    // instance Material à chaque frame via _renderer.material (qui clone le material).
    private MaterialPropertyBlock _mpb;

    // ═══════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider  = GetComponent<Collider>();
        _mpb       = new MaterialPropertyBlock();
    }

    public override void OnNetworkSpawn()
    {
        _isActive.OnValueChanged += OnIsActiveChanged;
        TargetManager.Instance?.RegisterTarget(this);
        ApplyState(_isActive.Value, immediate: true);
    }

    public override void OnNetworkDespawn()
    {
        _isActive.OnValueChanged -= OnIsActiveChanged;
        TargetManager.Instance?.UnregisterTarget(this);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RPCs
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>AR : demande l'activation d'une cible inactive.</summary>
    [ServerRpc(RequireOwnership = false)]
    public void ActivateByARServerRpc()
    {
        if (!_isActive.Value)
            _isActive.Value = true;
    }

    /// <summary>VR : déclare avoir touché la cible active.</summary>
    [ServerRpc(RequireOwnership = false)]
    public void HitByVRServerRpc(ulong vrClientId)
    {
        if (!_isActive.Value) return;

        _isActive.Value = false;
        OnTargetHit?.Invoke(vrClientId);
        Debug.Log($"[TargetController] Cible {NetworkObjectId} touchée par client {vrClientId}");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // State handling
    // ═══════════════════════════════════════════════════════════════════════════

    private void OnIsActiveChanged(bool _, bool newValue)
    {
        ApplyState(newValue, immediate: false);
        // Invalider le cache du TargetManager pour éviter l'allocation LateUpdate.
        TargetManager.Instance?.NotifyStateChanged();
    }

    private void ApplyState(bool active, bool immediate)
    {
        if (_collider != null)
            _collider.enabled = active;

        gameObject.layer = active ? LayerActive : LayerInactive;

        if (_renderer != null)
        {
            Material mat = active ? activeMaterial : inactiveMaterial;
            // sharedMaterial : N'instancie pas de copie — crucial pour les performances.
            if (mat != null) _renderer.sharedMaterial = mat;
            // Réinitialiser le PropertyBlock pour que la couleur du sharedMaterial s'applique.
            _renderer.SetPropertyBlock(null);

            if (active && !immediate)
                StartCoroutine(PulseCoroutine());
        }
    }

    private IEnumerator PulseCoroutine()
    {
        if (_renderer == null || activeMaterial == null) yield break;

        // Lire la couleur depuis le sharedMaterial (pas d'instanciation).
        Color baseColor = activeMaterial.color;
        float elapsed   = 0f;

        while (elapsed < activationPulseDuration)
        {
            elapsed += Time.deltaTime;
            float intensity = 1f + 0.5f * Mathf.Sin(elapsed / activationPulseDuration * Mathf.PI);
            // MaterialPropertyBlock : modifie la couleur sans créer une nouvelle instance Material.
            _mpb.SetColor("_BaseColor", baseColor * intensity);
            _renderer.SetPropertyBlock(_mpb);
            yield return null;
        }

        // Fin du pulse : réinitialiser les overrides du PropertyBlock.
        _renderer.SetPropertyBlock(null);
    }
}