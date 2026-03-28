using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultipleImageTracker : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsToSpawn; // Glissez vos prefabs ici
    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        if (trackedImageManager == null)
            Debug.LogError("[MultipleImageTracker] ARTrackedImageManager introuvable sur ce GameObject.", this);
    }

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.RemoveListener(OnChanged);
    }

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Pour chaque nouvelle image détectée
        foreach (var newImage in eventArgs.added)
        {
            string imageName = newImage.referenceImage.name;

            // Nettoyer une entrée obsolète (prefab détruit lors d'une session précédente)
            if (spawnedPrefabs.TryGetValue(imageName, out var existing) && existing == null)
                spawnedPrefabs.Remove(imageName);

            if (!spawnedPrefabs.ContainsKey(imageName) && prefabsToSpawn != null)
            {
                foreach (var prefab in prefabsToSpawn)
                {
                    if (prefab != null && prefab.name == imageName)
                    {
                        var spawned = Instantiate(prefab, newImage.transform);
                        spawnedPrefabs.Add(imageName, spawned);
                        break;
                    }
                }
            }
        }

        // Mise à jour de la visibilité selon l'état du tracking
        foreach (var updatedImage in eventArgs.updated)
        {
            string imageName = updatedImage.referenceImage.name;
            if (spawnedPrefabs.TryGetValue(imageName, out var obj) && obj != null)
                obj.SetActive(updatedImage.trackingState == TrackingState.Tracking);
        }

        // Nettoyage des images perdues
        foreach (var removedImage in eventArgs.removed)
        {
            string imageName = removedImage.Value.referenceImage.name;
            if (spawnedPrefabs.TryGetValue(imageName, out var obj))
            {
                if (obj != null)
                    Destroy(obj);
                spawnedPrefabs.Remove(imageName);
            }
        }
    }
}