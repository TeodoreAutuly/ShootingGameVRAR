using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// MANAGER AR — Côté client AR. Distributed Authority.
///
/// Expose :
///   - OnInstanceReady       : déclenché quand l'Instance est prête (utile si le board
///                             est instancié avant la connexion réseau).
///   - OnTargetsChanged      : déclenché à chaque spawn ou destruction de target locale.
/// </summary>
public class NetworkedTargetsManagerAR : NetworkBehaviour
{
    // ── Singleton + events statiques ─────────────────────────────────────────
    public static NetworkedTargetsManagerAR Instance { get; private set; }

    /// Déclenché quand l'Instance est assignée (OnNetworkSpawn).
    /// Permet au Bootstrapper de s'abonner même si le board est apparu avant la connexion.
    public static event Action<NetworkedTargetsManagerAR> OnInstanceReady;

    // ── Event instance ────────────────────────────────────────────────────────
    /// Déclenché à chaque changement de la liste des targets locales.
    public event Action<IReadOnlyList<Vector3>> OnTargetsChanged;

    [Header("Setup")]
    [SerializeField] private GameObject             _arTargetPrefab;
    [SerializeField] private PosesGenerationService _posesService;
    [SerializeField] private int                    _initialTargetCount = 10;

    private readonly Dictionary<Vector3, SingleTargetController> _localARTargets = new();

    // ── Lifecycle NGO / DA ───────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        Instance = this;

        // Notifie les abonnés en attente (ex: Bootstrapper apparu avant la connexion)
        OnInstanceReady?.Invoke(this);

        if (!IsOwner) return;

        SpawnLocalARTargets();
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
        DestroyAllARTargets();
    }

    // ── Spawn initial ─────────────────────────────────────────────────────────

    private void SpawnLocalARTargets()
    {
        var positions = _posesService.GeneratePoses(_initialTargetCount);
        Debug.Log($"AR Target Poses ({positions.Count}) : {string.Join(", ", positions)}");

        foreach (var pos in positions)
            SpawnARTarget(pos);

        NotifyTargetsChanged();
        Debug.Log($"[AR] {_localARTargets.Count} targets AR locales spawnées.");
    }

    private void SpawnARTarget(Vector3 position)
    {
        if (_localARTargets.ContainsKey(position)) return;

        GameObject go = Instantiate(_arTargetPrefab, position, Quaternion.identity);
        var controller = go.GetComponent<SingleTargetController>();
        controller.InitializeAsAR(this);
        _localARTargets[position] = controller;
    }

    // ── Appelé par SingleTargetController ────────────────────────────────────

    public void NotifyARHit(Vector3 position)
    {
        // DestroyARTarget(position);
        NotifyARHitRpc(position);
    }

    private void DestroyARTarget(Vector3 position)
    {
        if (_localARTargets.TryGetValue(position, out var controller))
        {
            _localARTargets.Remove(position);
            if (controller != null)
                Destroy(controller.gameObject);

            NotifyTargetsChanged();
        }
    }

    // ── Rpc ──────────────────────────────────────────────────────────────────

    [Rpc(SendTo.Authority)]
    private void NotifyARHitRpc(Vector3 position)
    {
        NetworkedTargetsManagerVR.Instance?.HandleARHit(position);
    }

    [Rpc(SendTo.NotAuthority)]
    public void TargetDestroyedByVRRpc(Vector3 position)
    {
        if (_localARTargets.ContainsKey(position))
        {
            Debug.LogWarning($"[AR] Nettoyage tardif de la target en {position}.");
            DestroyARTarget(position);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void NotifyTargetsChanged()
    {
        var positions = new List<Vector3>(_localARTargets.Keys);
        Debug.Log("AR Dots positions");
        Debug.Log(positions); //DEBUG POSITION
        OnTargetsChanged?.Invoke(positions);
    }

    private void DestroyAllARTargets()
    {
        foreach (var controller in _localARTargets.Values)
            if (controller != null) Destroy(controller.gameObject);
        _localARTargets.Clear();
        NotifyTargetsChanged();
    }

    public int LocalARTargetCount => _localARTargets.Count;
}