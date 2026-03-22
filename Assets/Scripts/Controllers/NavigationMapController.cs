using UnityEngine;

public class ARNavigationMapController
{
    private readonly ARNavigationMapModel _model;
    private readonly ARNavigationMapView _view;
    private readonly Transform _xrOrigin;
    private readonly Transform _arCamera;

    public ARNavigationMapController(
        ARNavigationMapModel model,
        ARNavigationMapView view,
        Transform xrOrigin,
        Transform arCamera)
    {
        _model = model;
        _view = view;
        _xrOrigin = xrOrigin;
        _arCamera = arCamera;
    }

    public void Tick()
    {
        if (_xrOrigin == null || _arCamera == null)
            return;

        // Rotation Y de la caméra = direction du regard sur le plan horizontal
        float rotationY = _arCamera.eulerAngles.y;

        _model.UpdateFromWorldPosition(_xrOrigin.position, rotationY);

        _view.UpdateArrow(_model.NormalizedPosition, _model.RotationY);
    }
}