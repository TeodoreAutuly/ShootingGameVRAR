using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayService : MonoBehaviour
{
    public static RelayService Instance { get; private set; }

    private string joinedLobbyId;
    private const string LOBBY_NAME = "VRAR_Lobby"; // Identifiant fixe du lobby

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async Task EnsureUnityServicesInitializedAndAuthenticated()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    /// <summary>
    /// Crée une session Relay, puis crée un Lobby public avec le même nom fixe
    /// pour héberger le code Relay de manière invisible.
    /// </summary>
    public async Task<string> CreateRelayAsync(int maxConnections = 2)
    {
        try
        {
            await EnsureUnityServicesInitializedAndAuthenticated();

            Debug.Log("[RelayService] Création de l'allocation Relay...");
            Allocation allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(maxConnections - 1);
            string joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            // Création du Lobby pour cacher le code
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(LOBBY_NAME, maxConnections, lobbyOptions);
            joinedLobbyId = lobby.Id;

            // Heartbeat du lobby pour le maintenir en vie
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15f));

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            Debug.Log($"[RelayService] Hébergement lancé ! Le code {joinCode} est caché dans le Lobby.");

            return joinCode; 
        }
        catch (Exception e)
        {
            Debug.LogError($"[RelayService] Erreur lors de la création : {e}\nInner Exception: {e.InnerException?.Message}");
            return null;
        }
    }

    /// <summary>
    /// Recherche automatiquement un lobby disponible, récupère le code Relay caché et s'y connecte.
    /// Il n'est plus nécessaire de passer un code manuel en paramètre.
    /// </summary>
    public async Task<bool> JoinRelayAutoAsync()
    {
        try
        {
            await EnsureUnityServicesInitializedAndAuthenticated();

            Debug.Log("[RelayService] Recherche d'une partie existante...");
            
            // Recherche simple des lobbies disponibles (Quick Join)
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobbyId = lobby.Id;

            // Récupération du fameux code Relay partagé par l'Hôte
            string joinCode = lobby.Data["RelayCode"].Value;
            Debug.Log($"[RelayService] Partie trouvée ! Auto-connexion au code Relay caché : {joinCode}");

            JoinAllocation joinAllocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            bool success = NetworkManager.Singleton.StartClient();
            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RelayService] Erreur lors de la jonction auto : {e}\nInner Exception: {e.InnerException?.Message}");
            return false;
        }
    }

    private System.Collections.IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnApplicationQuit()
    {
        if (!string.IsNullOrEmpty(joinedLobbyId))
        {
            try { LobbyService.Instance.RemovePlayerAsync(joinedLobbyId, AuthenticationService.Instance.PlayerId); }
            catch { /* Ignore sur la fermeture */ }
        }
    }
}