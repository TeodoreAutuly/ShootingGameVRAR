using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Prefabs à spawner")]
    public GameObject[] prefabs;

    [Header("Nombre d'objets à spawner")]
    public int spawnCount = 5;

    [Header("Centre de la zone de spawn")]
    [Tooltip("Position autour de laquelle les prefabs vont spawner")]
    public Vector3 spawnCenter = new Vector3(29.24f, 6f, 68.92f);

    [Header("Rayon de spawn autour du centre")]
    [Tooltip("Les prefabs spawneront dans ce rayon autour du centre")]
    public float spawnRadius = 15f;

    [Header("Hauteur fixe de spawn")]
    [Tooltip("Y fixe pour que les objets soient au sol")]
    public float spawnHeight = 6f;

    [Header("Distance minimale entre deux prefabs")]
    [Min(0f)]
    public float minDistanceBetweenPrefabs = 2f;

    [Tooltip("Nombre max de tentatives pour trouver une position valide")]
    public int maxAttempts = 30;

    private List<GameObject> _spawnedObjects = new List<GameObject>();

    public void SpawnPrefabs()
    {
        // 1. Détruire les anciens objets
        foreach (var obj in _spawnedObjects)
            if (obj != null)
                Destroy(obj);
        _spawnedObjects.Clear();

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("PrefabSpawner : aucun prefab assigné !");
            return;
        }

        // 2. Spawner autour du centre
        List<Vector3> usedPositions = new List<Vector3>();

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 candidatePos;
            bool validPosition = false;
            int attempts = 0;

            do
            {
                // Position aléatoire dans un cercle autour du centre
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                candidatePos = new Vector3(
                    spawnCenter.x + randomCircle.x,
                    spawnHeight,
                    spawnCenter.z + randomCircle.y
                );

                validPosition = IsFarEnough(candidatePos, usedPositions);
                attempts++;

            } while (!validPosition && attempts < maxAttempts);

            if (validPosition)
            {
                GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];
                Quaternion randomRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                GameObject spawned = Instantiate(prefabToSpawn, candidatePos, randomRot);
                _spawnedObjects.Add(spawned);
                usedPositions.Add(candidatePos);
            }
            else
            {
                Debug.LogWarning($"PrefabSpawner : impossible de placer le prefab {i + 1} " +
                                 $"après {maxAttempts} tentatives.");
            }
        }

        Debug.Log($"PrefabSpawner : {_spawnedObjects.Count}/{spawnCount} objets spawned.");
    }

    private bool IsFarEnough(Vector3 candidate, List<Vector3> usedPositions)
    {
        foreach (Vector3 pos in usedPositions)
            if (Vector3.Distance(candidate, pos) < minDistanceBetweenPrefabs)
                return false;
        return true;
    }
}
