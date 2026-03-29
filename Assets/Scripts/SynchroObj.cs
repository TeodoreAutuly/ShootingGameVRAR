using UnityEngine;
using Unity.Netcode;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkObject))]
public class SharedNetworkOrigin : NetworkBehaviour
{
    public static SharedNetworkOrigin Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    public Transform OriginTransform => transform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SharedNetworkOrigin] Une autre instance existe déjà. Celle-ci sera ignorée.", this);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (enableLogs)
        {
            Debug.Log(
                $"[SharedNetworkOrigin] Origine réseau disponible | IsSpawned={IsSpawned} | NetworkObjectId={NetworkObjectId}",
                this
            );
        }
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Vector3 WorldToRelativePosition(Vector3 worldPosition)
    {
        return transform.InverseTransformPoint(worldPosition);
    }

    public Vector3 RelativeToWorldPosition(Vector3 relativePosition)
    {
        return transform.TransformPoint(relativePosition);
    }

    public Quaternion WorldToRelativeRotation(Quaternion worldRotation)
    {
        return Quaternion.Inverse(transform.rotation) * worldRotation;
    }

    public Quaternion RelativeToWorldRotation(Quaternion relativeRotation)
    {
        return transform.rotation * relativeRotation;
    }
}