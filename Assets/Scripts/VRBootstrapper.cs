using UnityEngine;

public class VRBootstrapper : MonoBehaviour
{
    public static VRBootstrapper Instance { get; private set; }

    [Header("Local VR Rig")]
    [SerializeField] private GameObject localXrOriginRoot;
    [SerializeField] private Transform localHead;
    [SerializeField] private Transform localLeftHand;
    [SerializeField] private Transform localRightHand;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    public GameObject LocalXrOriginRoot => localXrOriginRoot;
    public Transform LocalHead => localHead;
    public Transform LocalLeftHand => localLeftHand;
    public Transform LocalRightHand => localRightHand;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (enableLogs)
            {
                Debug.LogWarning("[VRBootstrapper] Duplicate instance detected. Destroying this GameObject.", this);
            }

            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (enableLogs)
        {
            Debug.Log("[VRBootstrapper] Instance registered.", this);
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
        out GameObject xrOriginRoot,
        out Transform head,
        out Transform leftHand,
        out Transform rightHand)
    {
        xrOriginRoot = localXrOriginRoot;
        head = localHead;
        leftHand = localLeftHand;
        rightHand = localRightHand;

        bool isValid =
            xrOriginRoot != null &&
            head != null &&
            leftHand != null &&
            rightHand != null;

        if (!isValid && enableLogs)
        {
            Debug.LogWarning(
                "[VRBootstrapper] Missing one or more local VR rig references. " +
                $"XR Origin: {(xrOriginRoot != null)}, " +
                $"Head: {(head != null)}, " +
                $"LeftHand: {(leftHand != null)}, " +
                $"RightHand: {(rightHand != null)}",
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
            "[VRBootstrapper] Local rig references status: " +
            $"XR Origin: {(localXrOriginRoot != null)}, " +
            $"Head: {(localHead != null)}, " +
            $"LeftHand: {(localLeftHand != null)}, " +
            $"RightHand: {(localRightHand != null)}",
            this
        );
    }
}