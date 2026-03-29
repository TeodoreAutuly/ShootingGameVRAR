using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Placé dans la scène de jeu sur un NetworkObject (ex: "GameManager").
/// Chaque client exécute OnNetworkSpawn et demande automatiquement au serveur
/// de spawner son avatar en fonction du rôle détecté par DeviceDetectionService.
///
/// Setup Unity :
/// 1. Créer un GO "GameManager" dans la GameScene, lui ajouter NetworkObject + GameBootstrap.
/// 2. Enregistrer vrPlayerPrefab et arPlayerPrefab dans le NetworkManager (Network Prefabs List).
/// 3. Assigner vrSpawnPoint et arSpawnPoint (Transforms vides dans la scène).
/// </summary>
public class GameBootstrap : NetworkBehaviour
{
    [Header("Prefabs Joueurs (enregistrés dans le NetworkManager)")]
    [Tooltip("Prefab VR : XR Origin (XR Rig) + NetworkObject + VRPlayerController.")]
    [SerializeField] private GameObject vrPlayerPrefab;

    [Tooltip("Prefab AR : XR Origin (AR Rig) + NetworkObject + ARCalibrationManager.")]
    [SerializeField] private GameObject arPlayerPrefab;

    [Header("Points de Spawn")]
    [Tooltip("Position de spawn de l'avatar VR dans la scène.")]
    [SerializeField] private Transform vrSpawnPoint;

    [Tooltip("Position de spawn de l'avatar AR (drone) dans la scène.")]
    [SerializeField] private Transform arSpawnPoint;

    public override void OnNetworkSpawn()
    {
        // Chaque client (y compris le Host/serveur) demande son propre spawn.
        // CORRECTIF : GameBootstrap est un objet de scène dont l'owner est TOUJOURS le serveur.
        // L'ancien guard "if (!IsOwner) return" bloquait systématiquement le client AR.
        // clientId est passé en paramètre explicite pour éviter l'ambiguïté de ServerRpcParams
        // quand le Host s'appelle lui-même (LocalClientId == SenderClientId == 0).
        DeviceDetectionService.PlayerRole role = DeviceDetectionService.Instance.CurrentRole;
        ulong myClientId = NetworkManager.Singleton.LocalClientId;
        Debug.Log($"[GameBootstrap] Rôle local : {role} (clientId={myClientId}). Demande de spawn...");
        RequestSpawnServerRpc(myClientId, role);
    }

    // RequireOwnership = false : n'importe quel client peut invoquer ce RPC,
    // pas seulement l'owner du GameBootstrap (qui est toujours le serveur).
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnServerRpc(ulong clientId, DeviceDetectionService.PlayerRole role)
    {
        bool isVR = role == DeviceDetectionService.PlayerRole.VR_Shooter;

        GameObject prefab = isVR ? vrPlayerPrefab : arPlayerPrefab;
        if (prefab == null)
        {
            Debug.LogError($"[GameBootstrap] Prefab non assigné pour le rôle '{role}' !");
            return;
        }

        Transform spawnPoint = isVR ? vrSpawnPoint : arSpawnPoint;
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        GameObject instance = Instantiate(prefab, pos, rot);
        NetworkObject netObj = instance.GetComponent<NetworkObject>();

        if (netObj != null)
        {
            // destroyWithScene : true → l'avatar est détruit quand la scène se décharge
            netObj.SpawnWithOwnership(clientId, true);
            Debug.Log($"[GameBootstrap][Serveur] '{prefab.name}' spawné pour le client {clientId}.");
        }
        else
        {
            Debug.LogError($"[GameBootstrap] Le prefab '{prefab.name}' n'a pas de composant NetworkObject !");
            Destroy(instance);
        }
    }
}
