using UnityEngine;

public class ARNavigationMapModel
{
    // Position sur le board en coordonnées locales (-0.5 à 0.5)
    public Vector2 NormalizedPosition { get; private set; }
    public float RotationY { get; private set; }

    private readonly float _mapScale; // 1 unité board = X mètres réels

    public ARNavigationMapModel(float mapScale = 10f)
    {
        _mapScale = mapScale;
    }

    public void UpdateFromWorldPosition(Vector3 worldPosition, float worldRotationY)
    {
        // Convertit la position monde en position normalisée sur le board
        NormalizedPosition = new Vector2(
            worldPosition.x / _mapScale,
            worldPosition.z / _mapScale
        );

        RotationY = worldRotationY;
    }
}