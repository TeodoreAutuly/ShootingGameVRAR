using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

[DisallowMultipleComponent]
public class ARConnectionManager : MonoBehaviour
{
    [Header("Session")]
    [SerializeField] private string sessionName = "popo2"; 
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

    private async void Start()
    {
        await ConnectAsync();
    }

    public async Task ConnectAsync()
    {
        if (_isConnecting)
            return;

        if (_session != null)
        {
            if (enableLogs)
                Debug.Log("[ARConnectionManager] A session already exists.", this);
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
            Debug.LogError($"[ARConnectionManager] Connection failed: {ex}", this);
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
            Debug.Log($"[ARConnectionManager] Unity Services initialized with profile '{profileId}'.", this);
    }

    private async Task SignInAsync()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            return;

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (enableLogs)
        {
            Debug.Log(
                $"[ARConnectionManager] Signed in anonymously. PlayerId = {AuthenticationService.Instance.PlayerId}",
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
            Debug.Log($"[ARConnectionManager] CreateOrJoinSessionAsync('{sessionName}')", this);

        _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);

        if (enableLogs)
        {
            Debug.Log(
                $"[ARConnectionManager] Session ready. Name='{_session.Name}', Id='{_session.Id}'",
                this
            );
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (_networkManager.LocalClientId != clientId)
            return;

        Debug.Log($"[ARConnectionManager] Local client connected. ClientId = {clientId}", this);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (_networkManager.LocalClientId != clientId)
            return;

        Debug.LogWarning($"[ARConnectionManager] Local client disconnected.", this);
    }

    private void OnSessionOwnerPromoted(ulong clientId)
    {
        if (_networkManager.LocalClient != null && _networkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"[ARConnectionManager] Local client is now Session Owner.", this);
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

        if (_session != null)
        {
            try
            {
                await _session.LeaveAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ARConnectionManager] LeaveAsync failed: {ex.Message}", this);
            }

            _session = null;
        }
    }
}