using Unity.Netcode;       
using UnityEngine;

public class TargetSpawnRequester : NetworkBehaviour
{
    [SerializeField] private TargetSpawnManager targetSpawnManager;

    public void RequestSpawn(Vector3 arOriginPosition)
    {
        if (!IsSpawned || !NetworkManager.IsConnectedClient)
        {
            Debug.LogError("[TargetSpawnRequester] Non connecté au réseau.", this);
            return;
        }

        if (NetworkManager.LocalClient.IsSessionOwner)
        {
            Debug.LogWarning("[TargetSpawnRequester] Le Session Owner ne doit pas spawner les targets.", this);
            return;
        }

        SpawnRequestRpc(NetworkManager.LocalClientId, arOriginPosition);
    }

    [Rpc(SendTo.Server)]
    private void SpawnRequestRpc(ulong requestingClientId, Vector3 arOriginPosition)
    {
        if (!NetworkManager.LocalClient.IsSessionOwner)
            return;

        targetSpawnManager.SpawnTargets(requestingClientId, arOriginPosition);
    }
}