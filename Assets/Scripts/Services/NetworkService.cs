using System;
using Unity.Netcode;
using UnityEngine;

// Le service implémente l'interface INetworkService
public class NetworkService : MonoBehaviour, INetworkService
{
    // On peut typer le Singleton avec l'interface pour encore plus de propreté
    public static INetworkService Instance { get; private set; }

    public event Action<ulong> OnPlayerJoined;
    public event Action<ulong> OnPlayerLeft;
    public event Action OnDisconnectedFromServer;

    private void Awake()
    {
        // ... (Même logique de Singleton qu'avant, castée si besoin)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
            Debug.Log("[NetworkService] Initialisé et lié au NetworkManager.");
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        OnPlayerJoined?.Invoke(clientId);
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
            OnDisconnectedFromServer?.Invoke();
        else
            OnPlayerLeft?.Invoke(clientId);
    }

    public bool StartHost() => NetworkManager.Singleton.StartHost();
    
    public bool StartClient() => NetworkManager.Singleton.StartClient();

    public void Disconnect() => NetworkManager.Singleton.Shutdown();

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }
}