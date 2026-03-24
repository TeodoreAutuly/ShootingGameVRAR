using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// MANAGER VR — Côté SessionOwner VR. Distributed Authority.
///
/// Cycle de vie :
///   1. Spawné par ConnectionManager (VR) avec ownership VR (SpawnWithOwnership).
///   2. Attend les Rpc(SendTo.Authority) depuis AR.
///   3. Quand AR touche une target → spawne la target localement côté VR.
///   4. Quand VRBullet détruit la target → détruit localement → Rpc(SendTo.NotAuthority) vers AR.
///
/// Setup Unity :
///   - Créer un prefab "TargetsManagerVR" avec NetworkObject + ce script.
///   - L'assigner dans ConnectionManager (VR)._targetsManagerPrefab.
///   - Assigner _vrTargetPrefab (prefab local, sans NetworkObject, Side=VR).
/// </summary>
public class NetworkedTargetsManagerVR : NetworkBehaviour
{
    public static NetworkedTargetsManagerVR Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] private GameObject _vrTargetPrefab;

    // Targets VR actives — locales uniquement, pas de NetworkObject.
    private readonly Dictionary<Vector3, SingleTargetController> _vrTargets = new();

    // ── Lifecycle NGO / DA ───────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        Instance = this;

        // En DA, seul le propriétaire (VR) gère les targets VR.
        // Les autres clients n'ont rien à faire ici.
        if (!IsOwner) return;

        Debug.Log("[VR] NetworkedTargetsManagerVR prêt. En attente des hits AR.");
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
        DestroyAllVRTargets();
    }

    // ── Appelé par NetworkedTargetsManagerAR.NotifyARHitRpc ──────────────────

    /// Reçu depuis AR (via Rpc Authority) : spawne la target côté VR.
    public void HandleARHit(Vector3 position)
    {
        if (!IsOwner) return;

        if (_vrTargets.ContainsKey(position))
        {
            Debug.LogWarning($"[VR] Target déjà présente en {position}, hit AR ignoré.");
            return;
        }

        SpawnVRTarget(position);
        Debug.Log($"[VR] Target VR spawnée en {position} suite à un hit AR.");
    }

    private void SpawnVRTarget(Vector3 position)
    {
        GameObject go = Instantiate(_vrTargetPrefab, position, Quaternion.identity);
        var controller = go.GetComponent<SingleTargetController>();
        controller.InitializeAsVR(this);
        _vrTargets[position] = controller;
    }

    // ── Appelé par SingleTargetController quand VRBullet détruit une target ──

    public void NotifyVRHit(Vector3 position)
    {
        if (!IsOwner) return;

        // 1. Détruit la target VR locale.
        DestroyVRTarget(position);

        // 2. Notifie tous les non-Authority (AR) que la target est détruite.
        TargetDestroyedRpc(position);

        Debug.Log($"[VR] Target en {position} détruite. Rpc envoyé vers AR.");
    }

    private void DestroyVRTarget(Vector3 position)
    {
        if (_vrTargets.TryGetValue(position, out var controller))
        {
            _vrTargets.Remove(position);
            if (controller != null)
                Destroy(controller.gameObject);
        }
    }

    // ── Rpc vers les non-Authority (AR) ──────────────────────────────────────

    /// Broadcast vers tous les clients non-Authority (AR) :
    /// la target VR est détruite, AR peut nettoyer si nécessaire.
    [Rpc(SendTo.NotAuthority)]
    private void TargetDestroyedRpc(Vector3 position)
    {
        NetworkedTargetsManagerAR.Instance?.TargetDestroyedByVRRpc(position);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void DestroyAllVRTargets()
    {
        foreach (var controller in _vrTargets.Values)
            if (controller != null) Destroy(controller.gameObject);
        _vrTargets.Clear();
    }

    public int ActiveVRTargetCount => _vrTargets.Count;
}