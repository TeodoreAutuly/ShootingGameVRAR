using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TargetSpawnManager : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private NetworkObject targetPrefab;

    [Header("Dispersion")]
    [SerializeField] private int targetCount = 10;
    [SerializeField] private float dispersionRadius = 5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float spawnHeight = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    private readonly List<Vector3> _spawnedPositions = new();
    private TargetManager _targetManager;

    public TargetManager TargetManager => _targetManager;

    private void Awake()
    {
        _targetManager = new TargetManager();
    }

    // Appelé par TargetSpawnRequester depuis le Session Owner
    public void SpawnTargets(ulong ownerClientId, Vector3 center)
    {
        if (enableLogs)
            Debug.Log($"[TargetSpawnManager] Spawn de {targetCount} targets autour de {center}", this);

        for (int i = 0; i < targetCount; i++)
        {
            Vector3 position = GetRandomPosition(center);

            NetworkObject instance = Instantiate(targetPrefab, position, Quaternion.identity);
            instance.SpawnWithOwnership(ownerClientId);

            _spawnedPositions.Add(position);

            var view = instance.GetComponent<SharedTargetView>();
            var sync = instance.GetComponent<SharedTargetNetworkSync>();

            if (view != null && sync != null)
                _targetManager.Register(view, sync);
            else
                Debug.LogWarning($"[TargetSpawnManager] Composants manquants sur la target {i}.", this);
        }

        if (enableLogs)
            Debug.Log($"[TargetSpawnManager] Spawn terminé. {_targetManager.RemainingTargets} targets actives.", this);
    }

    private Vector3 GetRandomPosition(Vector3 center)
    {
        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * dispersionRadius;
            Vector3 candidate = new Vector3(
                center.x + randomCircle.x,
                center.y + spawnHeight,
                center.z + randomCircle.y
            );

            if (IsFarEnoughFromOthers(candidate))
                return candidate;
        }

        return new Vector3(
            center.x + Random.Range(-dispersionRadius, dispersionRadius),
            center.y + spawnHeight,
            center.z + Random.Range(-dispersionRadius, dispersionRadius)
        );
    }

    private bool IsFarEnoughFromOthers(Vector3 candidate)
    {
        foreach (var pos in _spawnedPositions)
        {
            if (Vector3.Distance(candidate, pos) < minDistance)
                return false;
        }

        return true;
    }
}