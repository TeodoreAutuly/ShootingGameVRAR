using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayService : MonoBehaviour
{
    // Pattern Singleton simple pour un accès facile depuis tes UI/Controllers
    public static RelayService Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Garde le service actif entre les scènes
    }

    /// <summary>
    /// Crée une session Relay et démarre le joueur en tant que Host (Serveur + Client).
    /// </summary>
    /// <param name="maxConnections">Nombre total de joueurs (2 pour ton projet AR/VR)</param>
    /// <returns>Le code d'invitation (Join Code) à partager à l'autre joueur, ou null si erreur.</returns>
    public async Task<string> CreateRelayAsync(int maxConnections = 2)
    {
        try
        {
            Debug.Log("[RelayService] Création de l'allocation Relay...");
            
            // 1. Créer l'allocation (attention : le paramètre est le nombre de connexions *en plus* de l'hôte)
            Allocation allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(maxConnections - 1);

            // 2. Obtenir le code d'invitation court (ex: "A4B7C9")
            string joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"[RelayService] Relay créé avec succès ! Join Code : {joinCode}");

            // 3. Configurer le transport réseau de Netcode (NGO) avec les données du Relay
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // 4. Démarrer le NetworkManager en tant que Host (Le joueur VR ou AR qui crée la partie)
            NetworkManager.Singleton.StartHost();

            return joinCode; // On retourne le code pour l'afficher sur l'UI de l'hôte
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[RelayService] Erreur lors de la création du Relay : {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Rejoint une session Relay existante via un code d'invitation et démarre en tant que Client.
    /// </summary>
    /// <param name="joinCode">Le code fourni par l'hôte</param>
    /// <returns>Vrai si la connexion a réussi, sinon Faux.</returns>
    public async Task<bool> JoinRelayAsync(string joinCode)
    {
        try
        {
            Debug.Log($"[RelayService] Tentative de connexion avec le code : {joinCode}...");
            
            // 1. Rejoindre l'allocation avec le code fourni
            JoinAllocation joinAllocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);

            // 2. Configurer le transport réseau de Netcode (NGO)
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // 3. Démarrer le NetworkManager en tant que Client
            bool success = NetworkManager.Singleton.StartClient();
            
            if (success)
            {
                Debug.Log("[RelayService] Connecté au Relay en tant que Client !");
            }
            else
            {
                Debug.LogError("[RelayService] Échec de StartClient.");
            }

            return success;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"[RelayService] Erreur lors de la jonction du Relay : {e.Message}");
            return false;
        }
    }
}