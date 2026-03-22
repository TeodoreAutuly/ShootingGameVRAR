using UnityEngine;

public class ARNavigationMapBootstrapper : MonoBehaviour
{
    [Header("Références XR")]
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform arCamera;

    [Header("Settings")]
    [SerializeField] private float mapScale = 10f;

    private ARNavigationMapController _controller;

    public void OnBoardInstantiated(ARNavigationMapView view)
    {
        if (xrOrigin == null || arCamera == null || view == null)
        {
            Debug.LogError("[NavigationMapBootstrapper] Références manquantes.", this);
            return;
        }

        var model = new ARNavigationMapModel(mapScale);
        _controller = new ARNavigationMapController(model, view, xrOrigin, arCamera);

        Debug.Log("[NavigationMapBootstrapper] NavigationMap initialisée.", this);
    }

    public void OnBoardLost()
    {
        _controller = null;
        Debug.Log("[NavigationMapBootstrapper] NavigationMap désactivée.", this);
    }

    private void Update()
    {
        _controller?.Tick();
    }
}