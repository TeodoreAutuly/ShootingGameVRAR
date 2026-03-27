using UnityEngine;

public class SharedTargetController
{
    private readonly SharedTargetView _view;
    private readonly SharedTargetNetworkSync _networkSync;
    private readonly System.Action<SharedTargetController> _onDestroyed;

    private bool _touchedByAR;

    public SharedTargetController(
        SharedTargetView view,
        SharedTargetNetworkSync networkSync,
        System.Action<SharedTargetController> onDestroyed)
    {
        _view = view;
        _networkSync = networkSync;
        _onDestroyed = onDestroyed;

        _view.OnTouchedByARPlayer += HandleTouchedByAR;
        _view.OnHitByBullet += HandleHitByBullet;
    }

    public void Dispose()
    {
        _view.OnTouchedByARPlayer -= HandleTouchedByAR;
        _view.OnHitByBullet -= HandleHitByBullet;
    }

    private void HandleTouchedByAR()
    {
        if (_touchedByAR)
            return;

        _touchedByAR = true;

        // Changement visuel local uniquement
        _view.ApplyTouchedVisual();

        // Demande la synchronisation de la layer sur le réseau
        _networkSync.MakeVisibleRpc();
    }

    private void HandleHitByBullet()
    {
        // Demande la destruction réseau au Session Owner
        _networkSync.RequestDespawnRpc();
        _onDestroyed?.Invoke(this);
    }
}