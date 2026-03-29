using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AppBootstrap : MonoBehaviour
{
    [Header("VR")]
    [Tooltip("Le XR Rig présent dans la scène, désactivé par défaut. Sera activé si le rôle détecté est VR_Shooter.")]
    [SerializeField] private GameObject vrXRRig;
    [Tooltip("La caméra principale de la scène. Sera désactivée au profit du XR Rig si le rôle détecté est VR_Shooter.")]
    [SerializeField] private Camera mainCamera;

    private async void Start()
    {
        Debug.Log("[Bootstrap] Démarrage de la séquence d'initialisation...");

        try
        {
            await InitializeUnityServicesAsync();
            await AuthenticatePlayerAsync();
            
            InitializeCustomServices();
            ActivateXRRigIfVR();

            Debug.Log("[Bootstrap] Séquence terminée avec succès. Attente de l'action du joueur (LobbyController)...");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Bootstrap] Échec de l'initialisation : {e.Message}");
            // TODO : Afficher une pop-up d'erreur au joueur avec un bouton "Réessayer"
        }
    }

    private async Task InitializeUnityServicesAsync()
    {
        // Optionnel mais recommandé : configurer un profil si tu testes avec plusieurs instances sur le même PC (ex: ParrelSync)
        var options = new InitializationOptions();
        // options.SetProfile("Player1"); 

        await UnityServices.InitializeAsync(options);
        Debug.Log("[Bootstrap] Unity Services initialisés.");
    }

    private async Task AuthenticatePlayerAsync()
    {
        // On vérifie si le joueur n'est pas déjà connecté
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            // Connexion anonyme : parfait pour un jeu VR/AR rapide sans création de compte complexe
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"[Bootstrap] Joueur authentifié (ID: {AuthenticationService.Instance.PlayerId})");
        }
    }

    private void InitializeCustomServices()
    {
        Debug.Log("[Bootstrap] Initialisation des services locaux...");
        DeviceDetectionService.Instance.Initialize();
    }

    private void ActivateXRRigIfVR()
    {
        bool isVR = DeviceDetectionService.Instance.CurrentRole == DeviceDetectionService.PlayerRole.VR_Shooter;

        if (isVR)
        {
            // Désactiver la caméra de la scène lobby (UI/menu).
            // Si mainCamera n'est pas assigné dans l'Inspector, on tombe back sur Camera.main.
            Camera camToDisable = mainCamera != null ? mainCamera : Camera.main;
            if (camToDisable != null)
            {
                camToDisable.gameObject.SetActive(false);
                Debug.Log($"[Bootstrap] Caméra '{camToDisable.name}' désactivée (rôle VR_Shooter).");
            }

            if (vrXRRig != null)
            {
                vrXRRig.SetActive(true);
                Debug.Log("[Bootstrap] XR Rig activé (rôle VR_Shooter).");
            }
            else
            {
                Debug.LogWarning("[Bootstrap] Rôle VR_Shooter détecté mais vrXRRig non assigné dans l'Inspector !");
            }
        }
    }
}