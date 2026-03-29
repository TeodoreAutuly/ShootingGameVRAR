using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Registre global des cibles présentes dans la scène.
///
/// Les cibles s'enregistrent elles-mêmes via RegisterTarget() dans leur OnNetworkSpawn.
/// Ce manager n'instancie rien — les cibles sont placées manuellement dans la scène.
///
/// Ajouter ce composant sur un GameObject "TargetManager" dans la GameScene
/// avec un NetworkObject (pas besoin que ce soit le serveur seul qui le spawne,
/// la scène le fait automatiquement).
/// </summary>
public class TargetManager : NetworkBehaviour
{
    public static TargetManager Instance { get; private set; }

    private readonly List<TargetController> _allTargets      = new List<TargetController>();
    // Cache pré-alloué — mis à jour uniquement quand un target change d'état.
    // Évite une allocation new List<> à chaque appel de GetActiveTargets() depuis LateUpdate.
    private readonly List<TargetController> _activeCache     = new List<TargetController>();
    private bool _activeCacheDirty = true;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ─── API ──────────────────────────────────────────────────────────────────

    /// <summary>Appelé par TargetController.OnNetworkSpawn().</summary>
    public void RegisterTarget(TargetController target)
    {
        if (!_allTargets.Contains(target))
        {
            _allTargets.Add(target);
            _activeCacheDirty = true;
        }
    }

    /// <summary>Appelé par TargetController.OnNetworkDespawn().</summary>
    public void UnregisterTarget(TargetController target)
    {
        _allTargets.Remove(target);
        _activeCacheDirty = true;
    }

    /// <summary>
    /// Appelé par TargetController.OnIsActiveChanged() à chaque changement d'état.
    /// Marque le cache comme invalide pour la prochaine lecture.
    /// </summary>
    public void NotifyStateChanged()
    {
        _activeCacheDirty = true;
    }

    /// <summary>
    /// Retourne la liste des cibles actives.
    /// Ne réalloue pas — le cache est rebuild uniquement quand un état a changé.
    /// </summary>
    public IReadOnlyList<TargetController> GetActiveTargets()
    {
        if (_activeCacheDirty)
        {
            _activeCache.Clear();
            foreach (TargetController t in _allTargets)
                if (t != null && t.IsActive)
                    _activeCache.Add(t);
            _activeCacheDirty = false;
        }
        return _activeCache;
    }

    /// <summary>Retourne toutes les cibles enregistrées (actives + inactives).</summary>
    public IReadOnlyList<TargetController> GetAllTargets() => _allTargets;
}