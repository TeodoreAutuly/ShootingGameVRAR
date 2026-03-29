using UnityEngine;

/// <summary>
/// Pilote le mesh du Cyborg avatar (UltimateXR) depuis les transforms XRI
/// déjà synchronisés par NetworkTransform.
///
/// Placer ce composant sur le GO "CyborgAvatar_URP" (enfant direct de la racine Player_VR).
/// N'est PAS un NetworkBehaviour : il lit des transforms déjà répliqués.
///
/// Hiérarchie attendue du prefab Player_VR :
/// ┌─ XR Origin (XR Rig)      [NetworkObject · NetworkTransform Server-auth · VRPlayerController]
/// │   ├─ Camera Offset
/// │   │   └─ Main Camera     [TrackedPoseDriver · NetworkTransform Owner-auth]
/// │   ├─ Left Controller     [ActionBasedController · NetworkTransform Owner-auth]
/// │   ├─ Right Controller    [ActionBasedController · NetworkTransform Owner-auth]
/// │   │   └─ GunAnchor       [GO vide → Gun.prefab glissé ici]
/// │   └─ CyborgAvatar_URP    [UxrAvatar · VRBodyDriver]
/// │       └─ ...mesh Cyborg...
///
/// Comportement :
/// - Positionne le corps de l'avatar au sol sous la tête (floor = headY - eyeHeight).
/// - Aligne le Yaw du corps sur le Yaw de la tête (le joueur "regarde" dans la bonne direction).
/// - Sur l'instance propriétaire : cache le mesh de la tête (vue FPS → pas de clipping).
/// - Sur les copies distantes (AR client) : affiche l'avatar complet.
///
/// La main droite du Cyborg est positionnée visuellement via le Right Controller (GunAnchor).
/// </summary>
public class VRBodyDriver : MonoBehaviour
{
    [Header("Sources XRI — auto-découvertes si vides")]
    [Tooltip("Main Camera (TrackedPoseDriver). Laissez vide : trouvé via VRPlayerController.HeadTransform.")]
    [SerializeField] private Transform headSource;

    [Header("Avatar")]
    [Tooltip("GO contenant le head mesh (à cacher en vue FPS). Généralement 'Head' ou 'CyborgGeo' dans la hiérarchie.")]
    [SerializeField] private GameObject headMeshGO;

    [Header("Configuration")]
    [Tooltip("Hauteur yeux par rapport au sol (mètres). Servira à recalculer la position du corps sous la tête.")]
    [SerializeField] private float eyeHeight = 1.65f;

    [Tooltip("Vitesse de rotation du corps (lerp Yaw) pour un suivi souple.")]
    [SerializeField] private float bodyTurnSmoothSpeed = 10f;

    private VRPlayerController _vrController;
    private bool _isOwner;

    private void Start()
    {
        _vrController = GetComponentInParent<VRPlayerController>();

        if (_vrController == null)
        {
            Debug.LogError("[VRBodyDriver] VRPlayerController introuvable dans les parents. " +
                           "Vérifiez que CyborgAvatar_URP est bien enfant de la racine Player_VR.");
            enabled = false;
            return;
        }

        // Auto-découverte de la tête
        if (headSource == null)
            headSource = _vrController.HeadTransform;

        // Auto-découverte du mesh de tête si non assigné : chercher "Head" dans la hiérarchie locale
        if (headMeshGO == null)
        {
            Transform found = FindChildByName(transform, "Head");
            if (found != null) headMeshGO = found.gameObject;
        }

        // IsOwner n'est disponible que via NetworkBehaviour → on le lit depuis le contrôleur parent
        _isOwner = _vrController.IsOwner;

        // Vue FPS : cacher le mesh de la tête pour éviter le clipping caméra
        if (_isOwner && headMeshGO != null)
        {
            headMeshGO.SetActive(false);
            Debug.Log("[VRBodyDriver] Vue FPS : mesh tête Cyborg masqué.");
        }
    }

    private void LateUpdate()
    {
        if (headSource == null) return;

        // ── Position : sol sous la tête ────────────────────────────────────
        Vector3 targetPos = headSource.position;
        targetPos.y = headSource.position.y - eyeHeight;
        transform.position = targetPos;

        // ── Rotation : Yaw du corps aligné sur Yaw de la tête ───────────────
        float targetYaw = headSource.eulerAngles.y;
        Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                               bodyTurnSmoothSpeed * Time.deltaTime);
    }

    private static Transform FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform found = FindChildByName(child, childName);
            if (found != null) return found;
        }
        return null;
    }
}
