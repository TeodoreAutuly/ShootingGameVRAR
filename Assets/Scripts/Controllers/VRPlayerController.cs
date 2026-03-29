using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine;
using UltimateXR.Avatar;

/// <summary>
/// Composant racine du prefab VR Joueur (à placer sur XR Origin (XR Rig)).
///
/// Rôle :
/// - L'XR Origin sert d'ANCRE FIXE dans l'espace mondial (placée par le serveur au spawn,
///   jamais bougée par des providers de locomotion).
/// - Le joueur VR se déplace uniquement physiquement (room-scale), la caméra et les mains
///   suivent le tracking du casque via TrackedPoseDriver — propriétaire seulement.
/// - Sur les copies distantes (ex: client AR), les positions arrivent par NetworkTransform,
///   les composants de tracking local sont désactivés.
/// - Expose HeadTransform pour que ARCalibrationManager puisse calibrer l'espace AR.
///
/// Setup Unity (sur le prefab VR, APRÈS avoir dupliqué XR Origin (XR Rig)) :
/// 1. Ajouter NetworkObject sur la racine.
/// 2. Ajouter NetworkTransform sur la racine     → Authority : Server (position ancrée).
/// 3. Ajouter NetworkTransform sur Main Camera   → Authority : Owner  (tracking tête).
/// 4. Ajouter NetworkTransform sur Left Controller  → Authority : Owner  (tracking main gauche).
/// 5. Ajouter NetworkTransform sur Right Controller → Authority : Owner  (tracking main droite).
/// 6. Assigner les champs ci-dessous dans l'Inspector du prefab.
/// </summary>
public class VRPlayerController : NetworkBehaviour
{
    [Header("Nœuds suivis — assigner dans le prefab")]
    [Tooltip("Camera Offset > Main Camera  (NetworkTransform Owner-auth requis sur ce GO).")]
    [SerializeField] private Transform headTransform;

    [Tooltip("Left Controller GO  (NetworkTransform Owner-auth requis sur ce GO).")]
    [SerializeField] private GameObject leftControllerGO;

    [Tooltip("Right Controller GO  (NetworkTransform Owner-auth requis sur ce GO).")]
    [SerializeField] private GameObject rightControllerGO;

    /// <summary>Position de la main gauche, synchronisée par NetworkTransform.</summary>
    public Transform LeftHandTransform  => leftControllerGO  != null ? leftControllerGO.transform  : null;

    /// <summary>Position de la main droite, synchronisée par NetworkTransform.</summary>
    public Transform RightHandTransform => rightControllerGO != null ? rightControllerGO.transform : null;

    [Header("Locomotion — désactivée pour tous")]
    [Tooltip("Les GameObjects enfants de locomotion à désactiver : Move, Gravity, Turn, Teleportation, Climb.")]
    [SerializeField] private GameObject[] locomotionObjects;

    [Header("Composants désactivés sur les copies distantes")]
    [Tooltip("Composants de tracking XR local à désactiver sur les non-propriétaires :\n" +
             "- TrackedPoseDriver (Main Camera)\n" +
             "- ActionBasedController (Left Controller)\n" +
             "- ActionBasedController (Right Controller)")]
    [SerializeField] private Behaviour[] localOnlyComponents;

    [Header("HUD — optionnel")]
    [Tooltip("VRHeadUpDisplay enfant de Main Camera. Activé uniquement pour le propriétaire.")]
    [SerializeField] private VRHeadUpDisplay hud;

    // ─── Score réseau ──────────────────────────────────────────────────────────
    /// <summary>Score du joueur VR, synchronisé à tous les clients (écriture : serveur seul).</summary>
    public NetworkVariable<int> Score { get; } = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // ─── Awake : auto-découverte ───────────────────────────────────────────────

    private void Awake()
    {
        // Tête : chercher la caméra dans "Camera Offset" en priorité pour éviter de
        // capturer la caméra UltimateXR (CyborgAvatar_URP/Camera Controller/Camera).
        if (headTransform == null)
        {
            Transform cameraOffset = FindChildByName(transform, "Camera Offset");
            Camera xrCam = cameraOffset?.GetComponentInChildren<Camera>(true);
            if (xrCam != null)
                headTransform = xrCam.transform;
            else
            {
                Camera cam = GetComponentInChildren<Camera>(true);
                if (cam != null) headTransform = cam.transform;
            }
        }

        // Contrôleurs : par nom
        if (leftControllerGO == null)
        {
            Transform found = transform.Find("Camera Offset")?.parent == null
                ? FindChildByName(transform, "Left Controller")
                : FindChildByName(transform, "Left Controller");
            if (found != null) leftControllerGO = found.gameObject;
        }

        if (rightControllerGO == null)
        {
            Transform found = FindChildByName(transform, "Right Controller");
            if (found != null) rightControllerGO = found.gameObject;
        }

        // HUD
        if (hud == null)
            hud = GetComponentInChildren<VRHeadUpDisplay>(true);

        // Locomotion : chercher le GO "Locomotion" et ses enfants
        if (locomotionObjects == null || locomotionObjects.Length == 0)
        {
            Transform loco = FindChildByName(transform, "Locomotion");
            if (loco != null) locomotionObjects = new[] { loco.gameObject };
        }

        // Composants locaux à désactiver sur les copies distantes
        if (localOnlyComponents == null || localOnlyComponents.Length == 0)
        {
            var list = new System.Collections.Generic.List<Behaviour>();
            list.AddRange(GetComponentsInChildren<TrackedPoseDriver>(true));
            list.AddRange(GetComponentsInChildren<InputActionManager>(true));
            // La Camera de CyborgAvatar (vue FPS des mains) ne doit pas être active
            // sur les copies distantes — elle rendrait une deuxième vue parasite.
            foreach (Camera cam in GetComponentsInChildren<Camera>(true))
                list.Add(cam);
            localOnlyComponents = list.ToArray();
        }
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

    /// <summary>
    /// Position de la tête du joueur VR, synchronisée par NetworkTransform.
    /// Utilisé par ARCalibrationManager comme point de référence pour la calibration du QR code.
    /// </summary>
    public Transform HeadTransform => headTransform;

    public override void OnNetworkSpawn()
    {
        // L'XR Origin ne bouge jamais via locomotion — il est l'ancre fixe du monde.
        DisableLocomotion();

        if (!IsOwner)
        {
            // Copie distante : les positions viennent de NetworkTransform.
            // Désactiver le tracking d'entrée local pour éviter tout conflit.
            DisableLocalInputTracking();
        }
        else
        {
            // ── Désactivation de CyborgAvatar_URP pour le joueur LOCAL (vue FPS) ────────────
            // Résout 3 bugs simultanément :
            //   1. Effet tunnel (bords noirs) : Camera Controller/Camera d'UltimateXR renderait
            //      en conflit avec la caméra XR principale → viewport parasité.
            //   2. Manettes flottantes : UltimateXR initialise l'avatar en T-pose avant que
            //      son IK converge vers les positions réellement trackées → modèles Quest
            //      se retrouvent au-dessus du joueur.
            //   3. Lag sévère (quelques FPS) : Full-Body IK d'UltimateXR tourne chaque frame
            //      pour le joueur local, totalement inutile en vue FPS.
            // Sur les copies distantes (client AR) : CyborgAvatar_URP reste ACTIF.
            //   VRBodyDriver le pilote via les transforms répliqués par NetworkTransform. ✓
            Transform cyborgAvatar = FindChildByName(transform, "CyborgAvatar_URP");
            if (cyborgAvatar != null)
            {
                cyborgAvatar.gameObject.SetActive(false);
                Debug.Log("[VRPlayerController] CyborgAvatar_URP désactivé (vue FPS locale — IK + caméra parasite supprimés).");
            }
            else
            {
                // Fallback si le GO est renommé : désactiver au moins Camera Controller
                Transform camCtrl = FindChildByName(transform, "Camera Controller");
                if (camCtrl != null)
                {
                    camCtrl.gameObject.SetActive(false);
                    Debug.LogWarning("[VRPlayerController] CyborgAvatar_URP introuvable — Camera Controller désactivé en fallback.");
                }
            }

            // Propriétaire : afficher le HUD et s'abonner au score
            if (hud != null) hud.ShowHUD();
            Score.OnValueChanged += OnScoreChanged;
        }

        // Le serveur écoute les hits de cibles pour tous les joueurs VR
        if (IsServer)
            TargetController.OnTargetHit += OnTargetHitServer;

        Debug.Log($"[VRPlayerController] Spawné. IsOwner={IsOwner}. " +
                  $"Tête prête pour calibration AR (NetworkObjectId={NetworkObjectId}).");
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            Score.OnValueChanged -= OnScoreChanged;
        if (IsServer)
            TargetController.OnTargetHit -= OnTargetHitServer;
    }

    // ─── Score handlers ────────────────────────────────────────────────────────

    private void OnTargetHitServer(ulong clientId)
    {
        if (clientId == OwnerClientId)
            Score.Value++;
    }

    private void OnScoreChanged(int _, int newScore)
    {
        if (hud != null) hud.UpdateScore(newScore);
    }

    // ─── Locomotion ────────────────────────────────────────────────────────────

    private void DisableLocomotion()
    {
        foreach (GameObject loco in locomotionObjects)
        {
            if (loco != null)
                loco.SetActive(false);
        }
    }

    // ─── Tracking local (copies distantes uniquement) ──────────────────────────

    private void DisableLocalInputTracking()
    {
        foreach (Behaviour comp in localOnlyComponents)
        {
            if (comp != null)
                comp.enabled = false;
        }

        // Mettre UltimateXR en mode "puppet" réseau : désactive IK full-body,
        // lecture des XR devices et animations automatiques sur cette copie distante.
        // Compatible avec VRBodyDriver qui pilote les transforms via NetworkTransform.
        UxrAvatar uxrAvatar = GetComponentInChildren<UxrAvatar>(true);
        if (uxrAvatar != null)
        {
            uxrAvatar.AvatarMode = UxrAvatarMode.UpdateExternally;
            Debug.Log("[VRPlayerController] UxrAvatar mis en mode UpdateExternally (copie distante).");
        }
    }
}
