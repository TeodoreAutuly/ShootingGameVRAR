using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Interactueur de cibles côté AR.
///
/// Placer ce composant sur la racine du prefab Player_AR.
///
/// Flux :
///   Toucher l'écran (ou clic souris en éditeur)
///   → ScreenPointToRay depuis la caméra AR
///   → Physics.Raycast layers 8 (Target_Inactive) + 9 (Target_Active)
///   → ActivateByARServerRpc() sur la cible touchée si elle est inactive
///
/// Setup Inspector :
///   - arCamera      : Caméra AR (ARCamera). Si null, utilise Camera.main.
///   - targetLayerMask : Layer 8 (Target_Inactive) | Layer 9 (Target_Active)
///   - maxRange      : portée max en mètres (défaut 100)
/// </summary>
public class ARTargetInteractor : NetworkBehaviour
{
    [Header("Références")]
    [Tooltip("Caméra AR principale. Si null, Camera.main est utilisée.")]
    [SerializeField] private Camera arCamera;

    [Header("Layers")]
    [Tooltip("Doit inclure Layer 8 (Target_Inactive) ET Layer 9 (Target_Active).")]
    [SerializeField] private LayerMask targetLayerMask;

    [Header("Paramètres")]
    [Tooltip("Portée maximum du rayon (mètres).")]
    [SerializeField] private float maxRange = 100f;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (arCamera == null)
            arCamera = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            enabled = false;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!TryGetTouchPosition(out Vector2 screenPos)) return;

        Camera cam = arCamera != null ? arCamera : Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, targetLayerMask))
        {
            TargetController target = hit.collider.GetComponentInParent<TargetController>();
            if (target != null && !target.IsActive)
            {
                target.ActivateByARServerRpc();
                Debug.Log($"[ARTargetInteractor] Activation demandée → cible {target.NetworkObjectId}");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Retourne true + la position écran s'il y a un nouveau toucher / clic.</summary>
    private static bool TryGetTouchPosition(out Vector2 position)
    {
        position = Vector2.zero;

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            position = Input.GetTouch(0).position;
            return true;
        }
        return false;
#else
        // Fallback éditeur / standalone : clic gauche
        if (Input.GetMouseButtonDown(0))
        {
            position = Input.mousePosition;
            return true;
        }
        return false;
#endif
    }
}
