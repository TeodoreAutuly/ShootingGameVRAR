using UnityEngine;

/// <summary>
/// CONTROLLER — Gère uniquement la flèche de direction sur le board AR.
/// X Les dots sont gérés directement par le Bootstrapper via ARTargetMapView.
/// </summary>
public class ARNavigationMapController
{
    private readonly ARNavigationMapModel _model;
    private readonly ARNavigationMapView  _view;
    private readonly Transform            _xrOrigin;
    private readonly Transform            _arCamera;

    public ARNavigationMapController(
        ARNavigationMapModel model,
        ARNavigationMapView  view,
        Transform            xrOrigin,
        Transform            arCamera)
    {
        _model    = model;
        _view     = view;
        _xrOrigin = xrOrigin;
        _arCamera = arCamera;
    }

    /// Appelé chaque frame par le Bootstrapper.
    public void Tick()
    {
        if (_arCamera == null) return;

        _model.UpdateRotation(_arCamera.eulerAngles.y);
        _view.UpdateArrow(_model.RotationY);
    }
}