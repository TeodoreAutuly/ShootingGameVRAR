using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LobbyController : MonoBehaviour
{
    [Header("UI References (To Be Assigned)")]
    [FormerlySerializedAs("joinCodeText")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private GameObject quitButton;
    
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private bool enableScenePrewarm = true;

    private bool isSceneEventSubscribed;
    private bool isPrewarmInProgress;
    private bool isPrewarmCompleted;
    private bool pendingLaunchAfterPrewarm;

    private void Start()
    {
        // On s'assure que le bouton Quitter est bien visible (s'il est assigné)
        if (quitButton != null) quitButton.SetActive(true);

        // Listen for client connection to enable the Start Game button for the host
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        if (enableScenePrewarm)
        {
            TryStartScenePrewarm();
        }

        // Initialisation de l'interface en fonction du rôle
        if (DeviceDetectionService.Instance.CurrentRole == DeviceDetectionService.PlayerRole.VR_Shooter)
        {
            Debug.Log("[Lobby] Joueur VR détecté : En attente d'une action via le menu Canvas (Créer la partie).");
        }
        else
        {
            Debug.Log("[Lobby] Joueur AR détecté : En attente d'une action via le menu Canvas (Rejoindre la partie).");
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            
            // On retire l'écouteur si la scène est détruite
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
                isSceneEventSubscribed = false;
            }
        }
    }

    /// <summary>
    /// Called by a UI Button "Host Game" (or automatically for VR)
    /// </summary>
    public async void OnHostButtonClicked()
    {
        await StartHostAsync();
    }

    private async Task StartHostAsync()
    {
        string joinCode = await RelayService.Instance.CreateRelayAsync(2);
        
        if (!string.IsNullOrEmpty(joinCode))
        {
            if (statusText != null) statusText.text = "Partie hébergée !\nEn attente du joueur AR...";
            Debug.Log("[LobbyController] Partie hébergée avec succès. En attente de connexion...");
            
            // Écouter le démarrage des scènes pour afficher la barre de chargement
            EnsureSceneEventListener();

            if (enableScenePrewarm)
            {
                TryStartScenePrewarm();
            }
        }
    }

    /// <summary>
    /// Called by a UI Button (AR player joining automatically)
    /// </summary>
    public async void OnJoinButtonClicked()
    {
        Debug.Log("[LobbyController] Tentative de connexion automatique à la partie...");
        
        // On met à jour l'interface pour patienter
        if (statusText != null) statusText.text = "Recherche de la partie...";

        bool success = await RelayService.Instance.JoinRelayAutoAsync();

        if (success)
        {
            Debug.Log("[LobbyController] Connecté avec succès.");
            if (statusText != null) statusText.text = "Connecté ! Synchronisation...";
            
            // Écouter le démarrage des scènes
            EnsureSceneEventListener();

            if (enableScenePrewarm)
            {
                TryStartScenePrewarm();
            }
        }
        else
        {
            if (statusText != null) statusText.text = "Échec : Aucune partie trouvée.";
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Si nous sommes l'Hôte (VR) et que le client (AR) se connecte
        if (NetworkManager.Singleton.IsServer && clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[LobbyController] Joueur AR connecté ! Lancement automatique de la scène de jeu.");
            if (statusText != null) statusText.text = "Joueur AR trouvé ! Chargement...";
            
            // On lance instantanément la scène de jeu
            RequestGameSceneLaunch();
        }
    }

    private void EnsureSceneEventListener()
    {
        if (isSceneEventSubscribed) return;
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null) return;

        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        isSceneEventSubscribed = true;
    }

    private void TryStartScenePrewarm()
    {
        if (isPrewarmCompleted || isPrewarmInProgress) return;
        StartCoroutine(PrewarmGameSceneCoroutine());
    }

    private System.Collections.IEnumerator PrewarmGameSceneCoroutine()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            yield break;
        }

        Scene scene = SceneManager.GetSceneByName(gameSceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            isPrewarmCompleted = true;
            yield break;
        }

        isPrewarmInProgress = true;
        Debug.Log($"[LobbyController] Préchargement de la scène '{gameSceneName}'...");

        AsyncOperation preloadLoadOp = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive);
        if (preloadLoadOp == null)
        {
            isPrewarmInProgress = false;
            yield break;
        }

        while (!preloadLoadOp.isDone)
        {
            yield return null;
        }

        AsyncOperation preloadUnloadOp = SceneManager.UnloadSceneAsync(gameSceneName);
        if (preloadUnloadOp != null)
        {
            while (!preloadUnloadOp.isDone)
            {
                yield return null;
            }
        }

        isPrewarmInProgress = false;
        isPrewarmCompleted = true;
        Debug.Log($"[LobbyController] Préchargement terminé pour '{gameSceneName}'.");

        if (pendingLaunchAfterPrewarm)
        {
            pendingLaunchAfterPrewarm = false;
            LaunchGameScene();
        }
    }

    private void RequestGameSceneLaunch()
    {
        if (enableScenePrewarm && isPrewarmInProgress)
        {
            pendingLaunchAfterPrewarm = true;
            if (statusText != null) statusText.text = "Préparation de la partie...";
            return;
        }

        LaunchGameScene();
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        // Si nous sommes l'Hôte (VR) et que le client (AR) s'est déconnecté avant le lancement complet
        if (NetworkManager.Singleton.IsServer && clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[LobbyController] Joueur AR (Client {clientId}) déconnecté !");
            if (statusText != null) statusText.text = "Le joueur AR s'est déconnecté.\nEn attente d'un nouveau joueur...";
        }
        // Si le serveur a fermé la connexion (Hôte VR déconnecté) ou si le client AR a perdu la connexion
        else if (clientId == 0 || clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[LobbyController] L'hôte VR s'est arrêté ou perte de connexion réseau.");
            
            // On s'assure que le NetworkManager de notre côté s'éteint proprement
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }

            if (statusText != null) statusText.text = "L'Hôte VR a fermé la partie.\nConnexion interrompue.";
        }
    }

    /// <summary>
    /// Lance le chargement asynchrone pour tous les joueurs (Appelé uniquement par l'Hôte)
    /// </summary>
    private void LaunchGameScene()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[LobbyController] Loading GameScene...");
            
            if (statusText != null) statusText.text = "Lancement de la partie : 0%";

            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        // On écoute le moment où le chargement de la scène commence pour NOUS
        if (sceneEvent.SceneEventType == SceneEventType.Load && sceneEvent.ClientId == NetworkManager.Singleton.LocalClientId)
        {
            if (sceneEvent.AsyncOperation != null)
            {
                StartCoroutine(UpdateLoadingText(sceneEvent.AsyncOperation));
            }
            else
            {
                // Si Unity/Netcode ne fournit pas d'AsyncOperation (fréquent pour l'Hôte local)
                if (statusText != null) statusText.text = "Préparation de l'environnement...";
            }
        }
        else if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted && sceneEvent.ClientId == NetworkManager.Singleton.LocalClientId)
        {
            // Sécurité : on indique la fin
            if (statusText != null) statusText.text = "Chargement terminé ! Déploiement...";
        }
    }

    private System.Collections.IEnumerator UpdateLoadingText(AsyncOperation asyncOp)
    {
        while (!asyncOp.isDone)
        {
            if (statusText != null)
            {
                // Unity charge jusqu'à 0.9 maximum, la dernière phase (0.9 à 1) active la scène
                // On normalise de 0 à 100%
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                int progressPercent = Mathf.RoundToInt(progress * 100f);
                statusText.text = $"Synchronisation de la carte... {progressPercent}%";
            }
            yield return null; // On attend la frame suivante
        }
    }

    /// <summary>
    /// Called by a UI Button "Quit"
    /// </summary>
    public void OnQuitButtonClicked()
    {
        Debug.Log("[LobbyController] Fermeture de l'application...");
        Application.Quit();
    }
}