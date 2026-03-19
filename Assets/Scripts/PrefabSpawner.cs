using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Prefabs à spawner")]
    public GameObject[] prefabs;           // Glisse tes prefabs ici dans l'Inspector

    [Header("Nombre d'objets à spawner")]
    public int spawnCount = 5;

    [Header("Zone de spawn (centre = position de ce GameObject)")]
    public Vector3 spawnAreaMin = new Vector3(-10f, 0f, -10f);
    public Vector3 spawnAreaMax = new Vector3( 10f, 0f,  10f);

    // Garde une référence aux objets spawned pour pouvoir les détruire
    private List<GameObject> _spawnedObjects = new List<GameObject>();

    public void SpawnPrefabs()
    {
        // 1. Détruire les anciens objets
        foreach (var obj in _spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        _spawnedObjects.Clear();

        // 2. Spawner de nouveaux objets à des positions aléatoires
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("PrefabSpawner : aucun prefab assigné !");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            // Choisir un prefab au hasard dans le tableau
            GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];

            // Position aléatoire dans la zone définie
            Vector3 randomPos = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );

            // Rotation aléatoire (optionnel)
            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            GameObject spawned = Instantiate(prefabToSpawn, randomPos, randomRot);
            _spawnedObjects.Add(spawned);
        }

        Debug.Log($"PrefabSpawner : {spawnCount} objets spawned !");
    }
}
