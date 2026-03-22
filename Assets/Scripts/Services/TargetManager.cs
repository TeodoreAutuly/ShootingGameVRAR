using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Gère la génération et l'état global des cibles sur le serveur (VR).
/// </summary>
public class TargetManager : NetworkBehaviour
{
    public static TargetManager Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("Le Prefab d'une Cible (Doit avoir un NetworkObject).")]
    [SerializeField] private GameObject targetPrefab;
    [Tooltip("Emplacements prédéfinis dans la scène VR pour instancier les cibles.")]
    [SerializeField] private Transform[] spawnPoints;

    private void Awake()
    {
        if (Instance != null && Instance != this) return;
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        // Seul le serveur gère la génération des cibles
        if (IsServer)
        {
            GenerateTargets();
        }
    }

    private void GenerateTargets()
    {
        if (targetPrefab == null || spawnPoints.Length == 0) return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            GameObject target = Instantiate(targetPrefab, spawnPoint.position, spawnPoint.rotation);
            // Par défaut, la cible est cachée/désactivée jusqu'à ce que l'AR l'active
            NetworkObject netObj = target.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(); // Fait apparaître la cible chez tous les clients
            }
        }
    }
}