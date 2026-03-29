using UnityEngine;

public class ARBootstrapper : MonoBehaviour
{
    public static ARBootstrapper Instance { get; private set; }

    [Header("Local AR Rig")]
    [SerializeField] private GameObject localArXrOriginRoot;
    [SerializeField] private Transform localArCamera;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    public GameObject LocalArXrOriginRoot => localArXrOriginRoot;
    public Transform LocalArCamera => localArCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (enableLogs)
            {
                Debug.LogWarning("[ARBootstrapper] Duplicate instance detected. Destroying this GameObject.", this);
            }

            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (enableLogs)
        {
            Debug.Log("[ARBootstrapper] Instance registered.", this);
        }

        ValidateReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool TryGetLocalRigReferences(
        out GameObject arXrOriginRoot,
        out Transform arCamera)
    {
        arXrOriginRoot = localArXrOriginRoot;
        arCamera = localArCamera;

        bool isValid =
            arXrOriginRoot != null &&
            arCamera != null;

        if (!isValid && enableLogs)
        {
            Debug.LogWarning(
                "[ARBootstrapper] Missing one or more local AR rig references. " +
                $"AR XR Origin: {(arXrOriginRoot != null)}, " +
                $"AR Camera: {(arCamera != null)}",
                this
            );
        }

        return isValid;
    }

    private void ValidateReferences()
    {
        if (!enableLogs)
            return;

        Debug.Log(
            "[ARBootstrapper] Local rig references status: " +
            $"AR XR Origin: {(localArXrOriginRoot != null)}, " +
            $"AR Camera: {(localArCamera != null)}",
            this
        );
    }
}