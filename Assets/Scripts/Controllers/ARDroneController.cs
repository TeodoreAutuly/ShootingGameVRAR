using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contrôleur du joueur AR (drone).
/// À placer sur la RACINE du prefab Player_AR (= XR Origin AR Rig).
///
/// Architecture du prefab Player_AR :
/// ┌─ XR Origin (AR Rig)  [NetworkObject · ClientNetworkTransform · ARDroneController · ARCalibrationManager]
/// │   ├─ Camera Offset
/// │   │   └─ Main Camera  [Camera · ARCameraManager · ARCameraBackground · TrackedPoseDriver · ClientNetworkTransform]
/// │   └─ Drone            [← Drone.prefab (VoodooPlay) · ClientNetworkTransform]
///
/// Modes de déplacement :
/// ─ usePhysicalMovement = true (défaut) :
///     Le mesh Drone suit la position physique réelle du joueur (arCameraTransform.position en
///     world-space). Après calibration ARCalibrationManager, l'espace AR est aligné sur l'espace
///     VR, donc chaque pas physique du joueur déplace le drone dans la scène virtuelle.
///     Le joystick déplace en plus la RACINE XROrigin → combinaison physique + virtuel possible.
/// ─ usePhysicalMovement = false :
///     Déplacement joystick uniquement (déplace la racine XROrigin).
/// </summary>
public class ARDroneController : NetworkBehaviour
{
    [Header("Mouvement")]
    [Tooltip("Vitesse de déplacement du joystick (m/s).")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Mouvement Physique")]
    [Tooltip("Si activé, le mesh Drone suit en temps réel la position physique du joueur " +
             "(position de la caméra AR dans le monde VR, après calibration).\n" +
             "Les mouvements réels du joueur sont ainsi reflétés dans la scène virtuelle.")]
    [SerializeField] private bool usePhysicalMovement = true;

    [Tooltip("Décalage vertical du drone par rapport à la caméra AR (utile si tu veux " +
             "que le drone flotte légèrement au-dessus du téléphone).")]
    [SerializeField] private float droneHeightOffset = 0f;

    [Header("Input (UI — On-Screen Stick)")]
    [SerializeField] private InputActionReference moveInput;

    [Header("Références — auto-découvertes si non assignées")]
    [Tooltip("Main Camera enfant du prefab (Camera Offset > Main Camera). " +
             "Laissez vide : sera trouvée automatiquement via GetComponentInChildren.")]
    [SerializeField] private Transform arCameraTransform;

    [Tooltip("L'enfant 'Drone' contenant le mesh 3D VoodooPlay. " +
             "Laissez vide : sera trouvé automatiquement via le nom 'Drone'.")]
    [SerializeField] private Transform droneBody;

    // ─── Cache mouvement (seuils anti-spam réseau) ────────────────────────────
    // ClientNetworkTransform envoie un paquet réseau dès que la position/rotation change.
    // On n'applique le changement que si le delta dépasse le seuil → ~10-20 updates/s max.
    private Vector3 _lastDronePosition;
    private float   _lastDroneYaw;
    private const float PositionThreshold = 0.001f; // 1 mm
    private const float RotationThreshold = 0.1f;   // 0.1 degré

    // ─── Initialisation ────────────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        // Auto-découverte de la caméra AR si non assignée dans l'Inspector.
        if (arCameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                arCameraTransform = cam.transform;
            else
                Debug.LogError("[ARDroneController] Aucune Camera trouvée dans le prefab AR !");
        }

        // Auto-découverte du corps drone si non assigné.
        if (droneBody == null)
        {
            Transform found = transform.Find("Drone");
            if (found != null)
                droneBody = found;
            else
                Debug.LogWarning("[ARDroneController] Enfant 'Drone' introuvable. " +
                                 "Glissez le prefab VoodooPlay/Prefab/Drone comme enfant de la racine et nommez-le 'Drone'.");
        }

        if (IsOwner)
        {
            // Rien à faire ici : ClientNetworkTransform sur la racine et le Drone
            // autorise l'Owner à envoyer ses transforms au réseau.
        }
        else
        {
            // Désactiver l'input sur les copies distantes.
            if (moveInput != null)
                moveInput.action.Disable();
        }
    }

    // ─── Update (propriétaire uniquement) ─────────────────────────────────────

    private void Update()
    {
        if (!IsOwner) return;

        HandleJoystickMovement();
        if (usePhysicalMovement) ApplyPhysicalMovement();
        ApplyHeadRotationToDrone();
    }

    /// <summary>
    /// Déplace la racine XR Origin dans le plan horizontal selon le joystick.
    /// Fonctionne en complément du mouvement physique si usePhysicalMovement est activé.
    /// </summary>
    private void HandleJoystickMovement()
    {
        if (moveInput == null || arCameraTransform == null) return;

        Vector2 inputDir = moveInput.action.ReadValue<Vector2>();
        if (inputDir.sqrMagnitude < 0.01f) return;

        Vector3 camForward = arCameraTransform.forward;
        Vector3 camRight   = arCameraTransform.right;
        camForward.y = 0f; camForward.Normalize();
        camRight.y   = 0f; camRight.Normalize();

        Vector3 moveDir = (camForward * inputDir.y + camRight * inputDir.x).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Ancre le mesh Drone sur la position physique réelle du joueur.
    ///
    /// Après la calibration (ARCalibrationManager), l'espace AR est aligné sur l'espace VR :
    /// arCameraTransform.position en world-space = position physique réelle du joueur
    /// convertie en coordonnées VR. Chaque pas physique se reflète donc dans la scène virtuelle.
    ///
    /// Note : le joystick déplace la RACINE (XROrigin), ce qui décale la baseline ; le drone
    /// suit toujours la caméra dans ce référentiel décalé — les deux s'additionnent naturellement.
    /// </summary>
    private void ApplyPhysicalMovement()
    {
        if (droneBody == null || arCameraTransform == null) return;

        Vector3 target = arCameraTransform.position;
        target.y += droneHeightOffset;

        // N'écrire la position que si le déplacement dépasse le seuil.
        // Évite de faire envoyer un paquet réseau par ClientNetworkTransform à chaque frame.
        if (Vector3.Distance(droneBody.position, target) > PositionThreshold)
        {
            droneBody.position = target;
            _lastDronePosition = target;
        }
    }

    /// <summary>
    /// Recopie le Yaw (rotation Y) du téléphone sur le corps 3D du drone.
    /// La synchronisation réseau est assurée par le ClientNetworkTransform placé sur l'enfant Drone.
    /// </summary>
    private void ApplyHeadRotationToDrone()
    {
        if (droneBody == null || arCameraTransform == null) return;

        float yaw = arCameraTransform.eulerAngles.y;

        // N'écrire la rotation que si le delta dépasse le seuil.
        if (Mathf.Abs(Mathf.DeltaAngle(_lastDroneYaw, yaw)) > RotationThreshold)
        {
            droneBody.rotation = Quaternion.Euler(0f, yaw, 0f);
            _lastDroneYaw = yaw;
        }
    }
}