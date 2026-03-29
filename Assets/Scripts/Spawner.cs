using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class LocalXRSpawner : MonoBehaviour
{
    [Header("Prefab à spawner")]
    [SerializeField] private GameObject prefabToSpawn;

    [Header("Spawn point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Propulsion")]
    [SerializeField] private Transform controller;
    [SerializeField] private float launchForce = 10f;

    [Header("Input Action")]
    [SerializeField] private InputActionProperty spawnAction;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    private void OnEnable()
    {
        if (spawnAction.action == null)
        {
            Debug.LogError("[LocalXRSpawner] Aucune InputAction assignée.", this);
            return;
        }

        spawnAction.action.Enable();

        if (enableLogs)
        {
            Debug.Log("[LocalXRSpawner] Input action activée.", this);
        }
    }

    private void OnDisable()
    {
        if (spawnAction.action != null)
        {
            spawnAction.action.Disable();

            if (enableLogs)
            {
                Debug.Log("[LocalXRSpawner] Input action désactivée.", this);
            }
        }
    }

    private void Update()
    {
        if (spawnAction.action == null)
        {
            return;
        }

        if (!spawnAction.action.WasPressedThisFrame())
        {
            return;
        }

        SpawnObject();
    }

    private void SpawnObject()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("[LocalXRSpawner] prefabToSpawn n'est pas assigné.", this);
            return;
        }

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogError("[LocalXRSpawner] Aucun NetworkManager actif.", this);
            return;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        GameObject instance = Instantiate(prefabToSpawn, position, rotation);

        if (instance == null)
        {
            Debug.LogError("[LocalXRSpawner] Échec de l'instanciation du prefab.", this);
            return;
        }

        NetworkObject networkObject = instance.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError(
                $"[LocalXRSpawner] Le prefab '{prefabToSpawn.name}' ne contient pas de NetworkObject.",
                this
            );
            Destroy(instance);
            return;
        }

        if (instance.TryGetComponent(out Rigidbody rb))
        {
            Transform dirSource = controller != null ? controller : (spawnPoint != null ? spawnPoint : transform);
            rb.linearVelocity = dirSource.forward * launchForce; // vélocité directe plutôt que AddForce
        }

        networkObject.Spawn();

        if (enableLogs)
        {
            Debug.Log(
                $"[LocalXRSpawner] Spawn réseau de '{instance.name}' en {position}.",
                instance
            );
        }
    }
}