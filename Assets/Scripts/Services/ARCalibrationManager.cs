using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Gère la synchronisation de l'espace AR avec le monde VR.
///
/// Placer ce composant sur la RACINE du prefab AR Joueur (XR Origin AR Rig).
///
/// Fonctionnement :
/// - S'active uniquement sur le client AR (vérifié via DeviceDetectionService).
/// - S'auto-assigne ses références (ARTrackedImageManager, racine AR) depuis le prefab.
/// - Attend que VRPlayerController soit spawné sur le réseau pour obtenir la position
///   de la tête VR comme point de référence.
/// - Quand le joueur AR scanne le QR code physique collé sur le casque VR,
///   aligne l'espace AR avec l'espace VR en appliquant un offset position + rotation Y.
///
/// Prérequis physiques :
/// - Coller un QR code sur le casque VR (image présente dans le Reference Image Library AR Foundation).
/// - Le prefab VR doit avoir un VRPlayerController avec HeadTransform assigné.
/// </summary>
public class ARCalibrationManager : MonoBehaviour
{
    // Toutes les références sont auto-découvertes depuis le prefab ou via le réseau.
    // Aucun champ SerializeField n'est requis pour l'usage normal.

    private ARTrackedImageManager trackedImageManager;
    private GameObject arSessionOriginObject; // = ce GameObject (racine du prefab AR)
    private Transform vrOriginMarker;         // = VRPlayerController.HeadTransform (sync réseau)

    private bool isCalibrated = false;
    private bool isReady = false; // true quand les deux références sont trouvées

    private void Start()
    {
        // Ce composant n'a de sens que sur le client AR.
        if (DeviceDetectionService.Instance.CurrentRole != DeviceDetectionService.PlayerRole.AR_Drone)
        {
            enabled = false;
            return;
        }

        // Auto-assignation des références locales depuis le prefab.
        arSessionOriginObject = gameObject;
        trackedImageManager = GetComponentInChildren<ARTrackedImageManager>();

        if (trackedImageManager == null)
        {
            Debug.LogError("[ARCalibration] ARTrackedImageManager introuvable dans le prefab AR. " +
                           "Vérifiez que le prefab AR contient un ARTrackedImageManager.");
            enabled = false;
            return;
        }

        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);

        // Attendre le spawn du joueur VR pour récupérer sa tête comme point de référence.
        StartCoroutine(WaitForVRPlayerHead());
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }
    }

    /// <summary>
    /// Attend qu'un VRPlayerController soit présent dans la scène (spawné par le serveur),
    /// puis récupère HeadTransform comme marqueur de référence pour la calibration.
    /// </summary>
    private IEnumerator WaitForVRPlayerHead()
    {
        VRPlayerController vrPlayer = null;
        while (vrPlayer == null)
        {
            vrPlayer = FindAnyObjectByType<VRPlayerController>();
            if (vrPlayer == null) yield return new WaitForSeconds(0.5f);
        }

        vrOriginMarker = vrPlayer.HeadTransform;
        isReady = true;
        Debug.Log("[ARCalibration] Joueur VR détecté. Scannez le QR code sur le casque VR pour calibrer l'espace AR.");
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (isCalibrated || !isReady) return;

        foreach (var trackedImage in eventArgs.added)
        {
            CalibrateSpace(trackedImage);
            break; // On calibre dès la première image détectée
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking && !isCalibrated)
            {
                CalibrateSpace(trackedImage);
                break;
            }
        }
    }

    private void CalibrateSpace(ARTrackedImage trackedImage)
    {
        if (vrOriginMarker == null)
        {
            Debug.LogWarning("[ARCalibration] Le joueur VR n'est pas encore spawné. Calibration reportée.");
            return;
        }

        // 1. Calculer le décalage (offset) de Position
        // On veut que la position du 'trackedImage' (QR code local) coïncide avec le 'vrOriginMarker' (Origine virtuelle)
        Vector3 positionOffset = vrOriginMarker.position - trackedImage.transform.position;

        // 2. Calculer le décalage (offset) de Rotation
        // On aligne le repère Y pour qu'ils regardent dans la même direction (sans modifier le tangage ou roulis de l'AR)
        Quaternion rotationOffset = Quaternion.Euler(0, vrOriginMarker.eulerAngles.y - trackedImage.transform.eulerAngles.y, 0);

        // 3. Appliquer le décalage à l'AR Session Origin
        arSessionOriginObject.transform.position += positionOffset;
        arSessionOriginObject.transform.RotateAround(trackedImage.transform.position, Vector3.up, rotationOffset.eulerAngles.y);

        isCalibrated = true;
        Debug.Log($"[ARCalibration] Espace calibré sur le QR Code '{trackedImage.referenceImage.name}'. " +
                  "L'espace AR est maintenant synchronisé avec le monde VR !");
    }
}