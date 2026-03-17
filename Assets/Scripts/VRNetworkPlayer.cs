using Unity.Netcode;
using UnityEngine;

public class VRNetworkPlayer : NetworkBehaviour
{
    [Header("Networked Proxies")]
    [SerializeField] private Transform networkedHeadProxy;
    [SerializeField] private Transform networkedLeftHandProxy;
    [SerializeField] private Transform networkedRightHandProxy;

    [Header("Optional Visual Root")]
    [SerializeField] private GameObject visualRoot;

    [Header("Owner Visibility")]
    [SerializeField] private bool hideVisualsForOwner = false;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    private Transform _localHead;
    private Transform _localLeftHand;
    private Transform _localRightHand;

    private bool _localRigBound;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (enableLogs)
        {
            Debug.Log(
                $"[VRNetworkPlayer] OnNetworkSpawn | Owner={IsOwner} | LocalClientId={NetworkManager.LocalClientId} | OwnerClientId={OwnerClientId}",
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
            Debug.Log("[VRNetworkPlayer] OnNetworkDespawn", this);
        }

        _localRigBound = false;
        _localHead = null;
        _localLeftHand = null;
        _localRightHand = null;

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

        CopyPose(_localHead, networkedHeadProxy);
        CopyPose(_localLeftHand, networkedLeftHandProxy);
        CopyPose(_localRightHand, networkedRightHandProxy);
    }

    private void BindLocalRig()
    {
        if (VRBootstrapper.Instance == null)
        {
            if (enableLogs)
            {
                Debug.LogWarning("[VRNetworkPlayer] VRBootstrapper.Instance is null. Binding deferred.", this);
            }

            _localRigBound = false;
            return;
        }

        bool success = VRBootstrapper.Instance.TryGetLocalRigReferences(
            out _,
            out _localHead,
            out _localLeftHand,
            out _localRightHand
        );

        _localRigBound = success;

        if (enableLogs)
        {
            Debug.Log(
                $"[VRNetworkPlayer] BindLocalRig result = {_localRigBound}",
                this
            );
        }
    }

    private void TryLateBindLocalRig()
    {
        if (VRBootstrapper.Instance == null)
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
                $"[VRNetworkPlayer] visualRoot active = {shouldShow}",
                this
            );
        }
    }

    private void ValidateProxyReferences()
    {
        if (!enableLogs)
            return;

        Debug.Log(
            "[VRNetworkPlayer] Proxy references status: " +
            $"HeadProxy: {(networkedHeadProxy != null)}, " +
            $"LeftHandProxy: {(networkedLeftHandProxy != null)}, " +
            $"RightHandProxy: {(networkedRightHandProxy != null)}",
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