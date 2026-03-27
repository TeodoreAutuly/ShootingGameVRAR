using Unity.Netcode;
using UnityEngine;

public class SharedTargetNetworkSync : NetworkBehaviour
{
    [Header("Layers")]
    [SerializeField] private string initialLayerName = "IgnoredByXR";
    [SerializeField] private string visibleLayerName = "Default";

    private NetworkVariable<int> _networkLayer = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private SharedTargetView _view;

    public override void OnNetworkSpawn()
    {
        _view = GetComponent<SharedTargetView>();

        // Applique la layer initiale
        int initialLayer = LayerMask.NameToLayer(initialLayerName);
        _networkLayer.Value = initialLayer;

        // S'abonne aux changements réseau
        _networkLayer.OnValueChanged += OnLayerChanged;

        // Applique la valeur courante
        _view?.SetLayer(_networkLayer.Value);
    }

    public override void OnNetworkDespawn()
    {
        _networkLayer.OnValueChanged -= OnLayerChanged;
    }

    private void OnLayerChanged(int previousLayer, int newLayer)
    {
        _view?.SetLayer(newLayer);
    }

    // Appelé par le controller AR pour rendre visible
    [Rpc(SendTo.Owner)]
    public void MakeVisibleRpc()
    {
        int visibleLayer = LayerMask.NameToLayer(visibleLayerName);
        _networkLayer.Value = visibleLayer;
    }

    // Appelé par le controller AR pour demander la destruction
    [Rpc(SendTo.Server)]
    public void RequestDespawnRpc()
    {
        if (IsSessionOwner)
            NetworkObject.Despawn(true);
    }
}