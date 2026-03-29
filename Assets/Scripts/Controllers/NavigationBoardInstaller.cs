using UnityEngine;

public class NavigationBoardInstaller : MonoBehaviour
{
    private void Start()
    {
        var bootstrapper = FindFirstObjectByType<ARNavigationMapBootstrapper>();
        if (bootstrapper == null)
        {
            Debug.LogError("[NavigationBoardInstaller] Bootstrapper introuvable.", this);
            return;
        }

        var view = GetComponentInChildren<ARNavigationMapView>();
        if (view == null)
        {
            Debug.LogError("[NavigationBoardInstaller] NavigationMapView introuvable.", this);
            return;
        }

        bootstrapper.OnBoardInstantiated(view);
        Debug.Log("[NavigationBoardInstaller] Board instancié, bootstrapper notifié.", this);
    }

    private void OnDestroy()
    {
        var bootstrapper = FindFirstObjectByType<ARNavigationMapBootstrapper>();
        bootstrapper?.OnBoardLost();
        Debug.Log("[NavigationBoardInstaller] Board détruit, bootstrapper notifié.", this);
    }
}  