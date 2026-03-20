using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AppBootstrap : MonoBehaviour
{
    private async void Start()
    {
        Debug.Log("[Bootstrap] Démarrage de la séquence d'initialisation...");

        try
        {
            await InitializeUnityServicesAsync();
            await AuthenticatePlayerAsync();
            
            InitializeCustomServices();

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
        // C'est ici que tu initialises tes propres services dans le bon ordre.
        // Exemple avec un pattern Singleton ou Service Locator :
        
        Debug.Log("[Bootstrap] Initialisation des services locaux...");
        
        DeviceDetectionService.Instance.Initialize();
        NetworkService.Instance.Initialize();
        
        // Note : Ces services ne font pas de requêtes réseau lourdes ici, 
        // ils se préparent juste à être utilisés par tes Controllers.
    }
}