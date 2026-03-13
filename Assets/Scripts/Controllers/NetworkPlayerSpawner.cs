using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerSpawner : NetworkBehaviour
{
    [Header("Prefabs des Joueurs")]
    [Tooltip("Le prefab contenant le XR Origin et les mains VR")]
    [SerializeField] private GameObject vrPlayerPrefab;
    
    [Tooltip("Le prefab contenant la caméra AR Session")]
    [SerializeField] private GameObject arPlayerPrefab;

    /// <summary>
    /// Cette méthode est appelée automatiquement par NGO quand cet objet apparaît sur le réseau.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // On s'assure que seul le propriétaire de cet objet exécute ce code
        // (Le joueur local qui vient de se connecter)
        if (IsOwner)
        {
            // 1. On lit le rôle détecté localement par notre service
            DeviceDetectionService.PlayerRole myRole = DeviceDetectionService.Instance.CurrentRole;
            
            Debug.Log($"[NetworkPlayerSpawner] Je suis connecté ! Mon rôle est : {myRole}. Demande de spawn au serveur...");

            // 2. On envoie une requête (Remote Procedure Call) au Serveur
            RequestSpawnAvatarServerRpc(myRole);
        }
    }

    /// <summary>
    /// [ServerRpc] indique que ce code est exécuté UNIQUEMENT sur le Serveur/Hôte,
    /// même si c'est le Client qui l'appelle.
    /// </summary>
    [ServerRpc(RequireOwnership = true)]
    private void RequestSpawnAvatarServerRpc(DeviceDetectionService.PlayerRole requestedRole, ServerRpcParams rpcParams = default)
    {
        // On récupère l'ID du client qui a fait la demande
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameObject prefabToSpawn = null;

        // On choisit le bon prefab en fonction du rôle reçu
        if (requestedRole == DeviceDetectionService.PlayerRole.VR_Shooter)
        {
            prefabToSpawn = vrPlayerPrefab;
            Debug.Log($"[Serveur] Spawn du rig VR pour le client {clientId}");
        }
        else if (requestedRole == DeviceDetectionService.PlayerRole.AR_Drone)
        {
            prefabToSpawn = arPlayerPrefab;
            Debug.Log($"[Serveur] Spawn du rig AR pour le client {clientId}");
        }
        else
        {
            Debug.LogWarning($"[Serveur] Rôle inconnu pour le client {clientId}, aucun avatar n'a été spawn.");
            return;
        }

        // On instancie le bon prefab sur le Serveur
        GameObject avatarInstance = Instantiate(prefabToSpawn);
        
        // On le synchronise sur le réseau en donnant le contrôle total (Ownership) au client
        NetworkObject networkObject = avatarInstance.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(clientId);
    }
}