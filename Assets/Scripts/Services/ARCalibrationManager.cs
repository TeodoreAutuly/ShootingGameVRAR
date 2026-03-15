using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Gère la synchronisation de l'espace AR avec le monde VR (Host).
/// </summary>
public class ARCalibrationManager : MonoBehaviour
{
    [Header("AR References")]
    [Tooltip("L'origine spatiale AR que l'on veut décaler.")]
    [SerializeField] private GameObject arSessionOriginObject;
    [Tooltip("Le gestionnaire d'images scannées d'AR Foundation")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("VR Server Reference")]
    [Tooltip("Le transform cible en VR (ex: un point virtuel de coordonnée fixe) représentant l'emplacement réel du QR code.")]
    [SerializeField] private Transform vrOriginMarker;

    private bool isCalibrated = false;

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (isCalibrated) return;

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
            Debug.LogError("[ARCalibration] vrOriginMarker est manquant ! Veuillez lui assigner l'origine partagée (0,0,0) en VR.");
            return;
        }

        // 1. Calculer le décalage (offset) de Position
        // On veut que la position du 'trackedImage' (QR code local) coïncide avec le 'vrOriginMarker' (Origine virtuelle)
        Vector3 positionOffset = vrOriginMarker.position - trackedImage.transform.position;

        // 2. Calculer le décalage (offset) de Rotation
        // On aligne le repère Y pour qu'ils regardent dans la même direction (sans modifier le tangage ou roulis de l'AR)
        Quaternion rotationOffset = Quaternion.Euler(0, vrOriginMarker.eulerAngles.y - trackedImage.transform.eulerAngles.y, 0);

        // 3. Appliquer le décalage à l'AR Session Origin
        if (arSessionOriginObject != null)
        {
            arSessionOriginObject.transform.position += positionOffset;
            
            // Note: Une rotation plus propre consisterait à faire pivoter l'AR Session Origin autour du point scanné
            arSessionOriginObject.transform.RotateAround(trackedImage.transform.position, Vector3.up, rotationOffset.eulerAngles.y);
            
            isCalibrated = true;
            Debug.Log($"[ARCalibration] Espace calibré avec succès sur le QR Code (Nom: {trackedImage.referenceImage.name}). L'espace AR est maintenant synchronisé avec le monde VR !");
        }
    }
}