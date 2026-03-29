using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contrôleur de tir VR.
///
/// Placer ce composant sur le nœud "Right Controller" dans le prefab Player_VR.
///
/// Flux :
///   Appui gâchette (InputAction) → Raycast depuis gunTip vers l'avant
///   → couche "Target_Active" (layer 9) → HitByVRServerRpc(localClientId)
///
/// Setup Inspector :
///   - triggerAction : InputActionReference pointant vers XRI / RightHand / Select (bool)
///   - gunTip        : Transform à l'extrémité du canon (enfant de GunAnchor)
///   - targetLayerMask : Layer 9 uniquement (Target_Active)
///   - maxRange      : portée max en mètres (défaut 50)
/// </summary>
public class VRShootingController : NetworkBehaviour
{
    [Header("Références")]
    [Tooltip("Origin du raycast — tip du pistolet (enfant de GunAnchor).")]
    [SerializeField] private Transform gunTip;

    [Tooltip("InputAction gâchette — XRI RightHand / Select.")]
    [SerializeField] private InputActionReference triggerAction;

    [Header("Tir")]
    [Tooltip("Masque de layer — doit contenir uniquement Target_Active (layer 9).")]
    [SerializeField] private LayerMask targetLayerMask;

    [Tooltip("Portée maximum du rayon (mètres).")]
    [SerializeField] private float maxRange = 50f;

    [Header("Feedback (optionnel)")]
    [Tooltip("AudioSource pour le son de tir.")]
    [SerializeField] private AudioSource shotAudio;

    // ── État interne ──────────────────────────────────────────────────────────
    private bool _wasPressed;

    // ─────────────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        triggerAction?.action.Enable();
    }

    private void OnDisable()
    {
        triggerAction?.action.Disable();
    }

    public override void OnNetworkSpawn()
    {
        // Le tir n'est géré que sur la machine propriétaire
        if (!IsOwner)
            enabled = false;
    }

    private void Update()
    {
        if (!IsOwner) return;

        bool pressed = triggerAction != null && triggerAction.action.IsPressed();

        if (pressed && !_wasPressed)
            Shoot();

        _wasPressed = pressed;
    }

    // ─────────────────────────────────────────────────────────────────────────

    private void Shoot()
    {
        Transform origin = gunTip != null ? gunTip : transform;
        shotAudio?.Play();

        Ray ray = new Ray(origin.position, origin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, targetLayerMask))
        {
            TargetController target = hit.collider.GetComponentInParent<TargetController>();
            if (target != null)
            {
                ulong localId = NetworkManager.Singleton.LocalClientId;
                target.HitByVRServerRpc(localId);
                Debug.Log($"[VRShootingController] Tir → cible {target.NetworkObjectId} (client {localId})");
            }
        }
    }

    // ─── Gizmo de débogage ────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Transform origin = gunTip != null ? gunTip : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin.position, origin.forward * maxRange);
    }
}
