using Unity.Netcode;
using UnityEngine;

public class ARNetworkPlayer : NetworkBehaviour
{
    [Header("Networked Proxy")]
    [SerializeField] private Transform networkedCameraProxy;

    [Header("Optional Visual Root")]
    [SerializeField] private GameObject visualRoot;

    [Header("Owner Visibility")]
    [SerializeField] private bool hideVisualsForOwner = false;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    private GameObject _localArXrOriginRoot;
    private Transform _localArCamera;
    private bool _localRigBound;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (enableLogs)
        {
            Debug.Log(
                $"[ARNetworkPlayer] OnNetworkSpawn | Owner={IsOwner} | LocalClientId={NetworkManager.LocalClientId} | OwnerClientId={OwnerClientId}",
                this
            );
        }

        if (IsOwner)
        {
            BindLocalRig();
        }

        ApplyVisibility();
        ValidateProxyReferences();
    }

    public override void OnNetworkDespawn()
    {
        if (enableLogs)
        {
            Debug.Log("[ARNetworkPlayer] OnNetworkDespawn", this);
        }

        _localRigBound = false;
        _localArXrOriginRoot = null;
        _localArCamera = null;

        base.OnNetworkDespawn();
    }

    private void LateUpdate()
    {
        if (!IsSpawned || !IsOwner)
            return;

        if (!_localRigBound)
        {
            TryLateBindLocalRig();
            return;
        }

        CopyPose(_localArCamera, networkedCameraProxy);
    }

    private void BindLocalRig()
    {
        if (ARBootstrapper.Instance == null)
        {
            if (enableLogs)
            {
                Debug.LogWarning("[ARNetworkPlayer] ARBootstrapper.Instance is null. Binding deferred.", this);
            }

            _localRigBound = false;
            return;
        }

        bool success = ARBootstrapper.Instance.TryGetLocalRigReferences(
            out _localArXrOriginRoot,
            out _localArCamera
        );

        _localRigBound = success;

        if (enableLogs)
        {
            Debug.Log(
                $"[ARNetworkPlayer] BindLocalRig result = {_localRigBound}",
                this
            );
        }
    }

    private void TryLateBindLocalRig()
    {
        if (ARBootstrapper.Instance == null)
            return;

        BindLocalRig();
    }

    private void ApplyVisibility()
    {
        if (visualRoot == null)
            return;

        bool shouldShow = !(IsOwner && hideVisualsForOwner);
        visualRoot.SetActive(shouldShow);

        if (enableLogs)
        {
            Debug.Log(
                $"[ARNetworkPlayer] visualRoot active = {shouldShow}",
                this
            );
        }
    }

    private void ValidateProxyReferences()
    {
        if (!enableLogs)
            return;

        Debug.Log(
            "[ARNetworkPlayer] Proxy references status: " +
            $"CameraProxy: {(networkedCameraProxy != null)}",
            this
        );
    }

    private static void CopyPose(Transform source, Transform target)
    {
        if (source == null || target == null)
            return;

        Transform parent = target.parent;

        if (parent == null)
        {
            target.position = source.position;
            target.rotation = source.rotation;
            return;
        }

        target.localPosition = parent.InverseTransformPoint(source.position);
        target.localRotation = Quaternion.Inverse(parent.rotation) * source.rotation;
    }
}