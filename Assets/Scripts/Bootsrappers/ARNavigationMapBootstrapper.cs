using UnityEngine;

/// <summary>
/// BOOTSTRAPPER — Câblage uniquement. Aucune logique métier.
///
/// Crée et pilote :
///   - ARNavigationMapController  → flèche (activée quand le board est détecté)
///   - ARTargetDotsController     → dots screen space (toujours actif)
/// </summary>
public class ARNavigationMapBootstrapper : MonoBehaviour
{
    [Header("Références XR")]
    [SerializeField] private Transform _xrOrigin;
    [SerializeField] private Transform _arCamera;

    [Header("Settings")]
    [Tooltip("Taille monde représentée par la carte (unités Unity).")]
    [SerializeField] private float _mapScale = 10f;

    [Header("Vues")]
    [SerializeField] private ARTargetMapView _targetMapView;

    private ARNavigationMapController _arrowController;
    private ARTargetDotsController    _dotsController;

    // ── Lifecycle Unity ───────────────────────────────────────────────────────

    private void Start()
    {
        if (NetworkedTargetsManagerAR.Instance != null)
            CreateDotsController(NetworkedTargetsManagerAR.Instance);
        else
            NetworkedTargetsManagerAR.OnInstanceReady += OnManagerReady;
    }

    private void Update()
    {
        _arrowController?.Tick();
        _dotsController?.Tick();
    }

    private void OnDestroy()
    {
        NetworkedTargetsManagerAR.OnInstanceReady -= OnManagerReady;
        _dotsController?.Dispose();
    }

    // ── Lifecycle board (flèche) ──────────────────────────────────────────────

    public void OnBoardInstantiated(ARNavigationMapView arrowView)
    {
        if (_xrOrigin == null || _arCamera == null || arrowView == null)
        {
            Debug.LogError("[NavigationMapBootstrapper] Références manquantes pour la flèche.", this);
            return;
        }

        var model = new ARNavigationMapModel(_mapScale);
        _arrowController = new ARNavigationMapController(model, arrowView, _xrOrigin, _arCamera);

        Debug.Log("[NavigationMapBootstrapper] Flèche initialisée.", this);
    }

    public void OnBoardLost()
    {
        _arrowController = null;
        Debug.Log("[NavigationMapBootstrapper] Flèche désactivée.", this);
    }

    // ── Création du controller dots ───────────────────────────────────────────

    private void OnManagerReady(NetworkedTargetsManagerAR manager)
    {
        NetworkedTargetsManagerAR.OnInstanceReady -= OnManagerReady;
        CreateDotsController(manager);
    }

    private void CreateDotsController(NetworkedTargetsManagerAR manager)
    {
        _dotsController = new ARTargetDotsController(_targetMapView, _xrOrigin, manager, _mapScale);
        Debug.Log("[NavigationMapBootstrapper] DotsController créé.", this);
    }
}