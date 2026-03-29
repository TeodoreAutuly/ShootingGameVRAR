using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SERVICE — Génère N positions aléatoires dans l'espace selon des paramètres.
/// ScriptableObject sans dépendance réseau, injectable dans les managers.
/// Créer l'asset via clic droit → Create > Targets > PosesGenerationService.
/// </summary>
[CreateAssetMenu(menuName = "Targets/PosesGenerationService")]
public class PosesGenerationService : ScriptableObject
{
    [Header("Paramètres de dispersion")]
    [Tooltip("Distance minimale entre deux points")]
    public float minDistanceBetweenPoints = 0.5f;

    [Tooltip("Distance maximale entre deux points")]
    public float maxDistanceBetweenPoints = 3f;

    [Tooltip("Rayon minimal depuis le centre")]
    public float minRadius = 1f;

    [Tooltip("Rayon maximal depuis le centre")]
    public float maxRadius = 5f;

    [Tooltip("Point central autour duquel les dispersions sont effectuées")]
    public Vector3 centerPoint = Vector3.zero;

    [Tooltip("Tentatives max pour placer un point sans chevauchement")]
    public int maxPlacementAttempts = 30;

    /// <summary>
    /// Génère exactement N positions en respectant les contraintes de dispersion.
    /// </summary>
    public List<Vector3> GeneratePoses(int count)
    {
        var positions = new List<Vector3>(count);

        for (int i = 0; i < count; i++)
        {
            Vector3? candidate = TryPlacePoint(positions);
            if (candidate.HasValue)
                positions.Add(candidate.Value);
            else
                Debug.LogWarning($"[PosesGenerationService] Impossible de placer le point {i} après {maxPlacementAttempts} tentatives.");
        }

        return positions;
    }

    private Vector3? TryPlacePoint(List<Vector3> existing)
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            float angle  = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(minRadius, maxRadius);
            Vector3 candidate = centerPoint + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            if (IsValidPosition(candidate, existing))
                return candidate;
        }
        return null;
    }

    private bool IsValidPosition(Vector3 candidate, List<Vector3> existing)
    {
        foreach (var pos in existing)
        {
            float dist = Vector3.Distance(candidate, pos);
            if (dist < minDistanceBetweenPoints || dist > maxDistanceBetweenPoints)
                return false;
        }
        return true;
    }
}