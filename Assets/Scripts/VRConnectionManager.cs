using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkManager))]
public class ConnectionManager : MonoBehaviour
{
    private string sessionName = "";

    [Header("Session")]
    [SerializeField] private CodeDisplayInstaller installer;
    [SerializeField] private CodeDisplayView vrLobbyView;
    [SerializeField] private NetworkObject _targetsManagerVRPrefab;
    [SerializeField] private int maxPlayers = 10;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    private NetworkManager _networkManager;
    private ISession _session;
    private bool _isConnecting;

    private void Awake()
    {
        _networkManager = GetComponent<NetworkManager>();

        _networkManager.OnClientConnectedCallback += OnClientConnected;
        _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        _networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
    }

    private void Start()
    {
        if (installer != null && installer.Controller != null)
        {
            installer.Controller.CodeGenerated += HandleCodeGenerated;
        }
        else
        {
            Debug.LogWarning("[VRConnectionManager] Installer ou Controller non disponible au Start.", this);
        }

        if (vrLobbyView == null )
        {
            Debug.LogWarning("[VRConnectionManager] vrLobbyView non disponible au Start.", this);
        }
    }

    public async Task ConnectAsync()
    {
        if (_isConnecting)
            return;

        if (string.IsNullOrWhiteSpace(sessionName))
        {
            Debug.LogWarning("[VRConnectionManager] Impossible de se connecter : sessionName est vide.", this);
            return;
        }

        if (_session != null)
        {
            if (enableLogs)
                Debug.Log("[VRConnectionManager] A session already exists.", this);
            return;
        }

        _isConnecting = true;

        try
        {
            await InitializeServicesAsync();
            await SignInAsync();
            await CreateOrJoinSessionAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[VRConnectionManager] Connection failed: {ex}", this);
        }
        finally
        {
            _isConnecting = false;
        }
    }

    private async Task InitializeServicesAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Uninitialized)
            return;

        string profileId = Guid.NewGuid().ToString("N")[..30];

        var options = new InitializationOptions();
        options.SetProfile(profileId);

        await UnityServices.InitializeAsync(options);

        if (enableLogs)
            Debug.Log($"[VRConnectionManager] Unity Services initialized with profile '{profileId}'.", this);
    }

    private async Task SignInAsync()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            return;

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (enableLogs)
        {
            Debug.Log(
                $"[VRConnectionManager] Signed in anonymously. PlayerId = {AuthenticationService.Instance.PlayerId}",
                this
            );
        }
    }

    private async Task CreateOrJoinSessionAsync()
    {
        var options = new SessionOptions
        {
            Name = sessionName,
            MaxPlayers = maxPlayers
        }
        .WithDistributedAuthorityNetwork();

        if (enableLogs)
            Debug.Log($"[VRConnectionManager] CreateOrJoinSessionAsync('{sessionName}')", this);

        _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);

        // Utiliser PlayerJoined plutôt que Changed
        _session.PlayerJoined += OnPlayerJoined;
        _session.PlayerLeaving += OnPlayerLeaving;

        // Vérifier immédiatement au cas où on serait déjà le 2e joueur
        CheckPlayerCount();

        if (enableLogs)
        {
            Debug.Log(
                $"[VRConnectionManager] Session ready. Name='{_session.Name}', Id='{_session.Id}'",
                this
            );
        }
    }

    private void OnPlayerJoined(string playerId)
    {
        if (enableLogs)
            Debug.Log($"[VRConnectionManager] Player joined: {playerId}. Total = {_session.PlayerCount}");

        CheckPlayerCount();
    }

    private void OnPlayerLeaving(string playerId)
    {
        if (enableLogs)
            Debug.Log($"[VRConnectionManager] Player leaving: {playerId}. Total = {_session.PlayerCount}");
    }

    private void CheckPlayerCount()
    {
        if (_session.PlayerCount >= 2)
            vrLobbyView.HideLobby();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (_networkManager.LocalClientId == clientId)
        {
            if (enableLogs)
                Debug.Log($"[VRConnectionManager] Local client connected. ClientId = {clientId}", this);
    
            // Spawn du manager de targets VR avec ownership VR
            if (_targetsManagerVRPrefab != null)
            {
                NetworkObject managerInstance = Instantiate(_targetsManagerVRPrefab);
                managerInstance.SpawnWithOwnership(clientId);
            }
            else
            {
                Debug.LogWarning("[VRConnectionManager] _targetsManagerVRPrefab non assigné.", this);
            }
        }
    
        int count = _networkManager.ConnectedClientsIds.Count;
    
        if (enableLogs)
            Debug.Log($"[VRConnectionManager] Joueurs connectés : {count}");
    
        if (count >= 2)
            vrLobbyView.HideLobby();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (_networkManager.LocalClientId != clientId)
            return;

        Debug.LogWarning("[VRConnectionManager] Local client disconnected.", this);
    }

    private void OnSessionOwnerPromoted(ulong clientId)
    {
        if (_networkManager.LocalClient != null && _networkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log("[VRConnectionManager] Local client is now Session Owner.", this);
        }
    }

    private async void OnDestroy()
    {
        if (_networkManager != null)
        {
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            _networkManager.OnSessionOwnerPromoted -= OnSessionOwnerPromoted;
        }

        if (installer != null && installer.Controller != null)
        {
            installer.Controller.CodeGenerated -= HandleCodeGenerated;
        }

        if (_session != null)
        {
            try
            {
                await _session.LeaveAsync();
                
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VRConnectionManager] LeaveAsync failed: {ex.Message}", this);
            }

            _session = null;
        }
    }

    private async void HandleCodeGenerated(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            Debug.LogWarning("[VRConnectionManager] CodeGenerated reçu, mais le code est vide.", this);
            return;
        }

        sessionName = code;

        if (enableLogs)
            Debug.Log($"[VRConnectionManager] Nouveau sessionName reçu : {sessionName}", this);

        await ConnectAsync();
    }
}