using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CONTROLLER — Gère la position des dots targets sur l'UI screen space.
///
/// S'abonne lui-même à NetworkedTargetsManagerAR.OnTargetsChanged.
/// Recalcule les positions canvas à chaque Tick car le player se déplace.
/// </summary>
public class ARTargetDotsController
{
    private readonly ARTargetMapView           _view;
    private readonly Transform                 _xrOrigin;
    private readonly float                     _mapScale;
    private readonly NetworkedTargetsManagerAR _manager;

    // Dernière liste connue — mise à jour sur OnTargetsChanged, relue à chaque Tick
    private IReadOnlyList<Vector3> _worldPositions = new List<Vector3>();

    public ARTargetDotsController(
        ARTargetMapView           view,
        Transform                 xrOrigin,
        NetworkedTargetsManagerAR manager,
        float                     mapScale = 10f)
    {
        _view     = view;
        _xrOrigin = xrOrigin;
        _manager  = manager;
        _mapScale = mapScale;

        _manager.OnTargetsChanged += OnTargetsChanged;
    }

    /// Appelé chaque frame par le Bootstrapper.
    /// Repositionne les dots selon la position courante du player.
    public void Tick()
    {
        if (_xrOrigin == null || _mapScale == 0f) return;

        var canvasPositions = WorldToCanvas(_worldPositions, _xrOrigin.position);
        _view.UpdateDots(canvasPositions);
    }

    /// À appeler depuis le Bootstrapper.OnDestroy pour libérer l'abonnement.
    public void Dispose()
    {
        if (_manager != null)
            _manager.OnTargetsChanged -= OnTargetsChanged;
    }

    // ── Callback manager ──────────────────────────────────────────────────────

    private void OnTargetsChanged(IReadOnlyList<Vector3> worldPositions)
    {
        // Copie défensive — évite les modifications concurrentes de la liste source
        _worldPositions = new List<Vector3>(worldPositions);
    }

    // ── Conversion world space → canvas pixels ────────────────────────────────

    private List<Vector2> WorldToCanvas(IReadOnlyList<Vector3> worldPositions, Vector3 playerPosition)
    {
        float radius = _view.MapRadius;
        float scale  = radius / (_mapScale * 0.5f);
        var   result = new List<Vector2>(worldPositions.Count);

        foreach (var worldPos in worldPositions)
        {
            Vector2 offset = new Vector2(
                worldPos.x - playerPosition.x,
                worldPos.z - playerPosition.z
            );

            Vector2 canvasPos = offset * scale;

            if (canvasPos.magnitude > radius)
                canvasPos = canvasPos.normalized * radius;

            result.Add(canvasPos);
        }

        return result;
    }
}