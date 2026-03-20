using Unity.Netcode;
using UnityEngine;

public class NetworkService : MonoBehaviour
{
    public static NetworkService Instance { get; private set; }

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
            Debug.Log("[NetworkService] Initialisé et lié au NetworkManager.");
        }
    }
}