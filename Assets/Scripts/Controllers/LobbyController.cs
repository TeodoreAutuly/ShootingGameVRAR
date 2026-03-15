using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    [Header("UI References (To Be Assigned)")]
    // References to UI Text/Inputs that you will create in the Unity Editor
    [SerializeField] private TMPro.TextMeshProUGUI joinCodeText;
    [SerializeField] private TMPro.TMP_InputField joinCodeInput;
    [SerializeField] private GameObject startGameButton;
    
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        // By default, hide the Start Game button until a client connects
        if (startGameButton != null) startGameButton.SetActive(false);

        // Listen for client connection to enable the Start Game button for the host
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }

        // Auto-detect role and show appropriate UI if necessary, 
        // or just rely on user clicking "Host" or "Join" buttons manually.
        // For a seamless experience, VR could auto-host on Start:
        if (DeviceDetectionService.Instance.CurrentRole == DeviceDetectionService.PlayerRole.VR_Shooter)
        {
            Debug.Log("[Lobby] VR Player detected. Auto-hosting...");
            _ = StartHostAsync();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
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
        
        if (!string.IsNullOrEmpty(joinCode) && joinCodeText != null)
        {
            joinCodeText.text = $"JOIN CODE: {joinCode}";
            Debug.Log($"[LobbyController] Ready. Join code is {joinCode}");
        }
    }

    /// <summary>
    /// Called by a UI Button (AR player entering the code)
    /// </summary>
    public async void OnJoinButtonClicked()
    {
        if (joinCodeInput == null || string.IsNullOrEmpty(joinCodeInput.text))
        {
            Debug.LogWarning("[LobbyController] No join code entered.");
            return;
        }

        string code = joinCodeInput.text.ToUpper();
        bool success = await RelayService.Instance.JoinRelayAsync(code);

        if (success)
        {
            Debug.Log("[LobbyController] Successfully connected to Host.");
            // Wait for host to load the scene...
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // If we are the server (host) and someone else connected
        if (NetworkManager.Singleton.IsServer && clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[LobbyController] Player {clientId} connected! Enabling Start Game button.");
            if (startGameButton != null) startGameButton.SetActive(true);
        }
    }

    /// <summary>
    /// Called by the "Start Game" button (Host only)
    /// </summary>
    public void OnStartGameButtonClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[LobbyController] Loading GameScene...");
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }
}