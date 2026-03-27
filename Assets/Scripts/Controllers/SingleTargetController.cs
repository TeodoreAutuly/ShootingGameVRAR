using System.Collections;
using UnityEngine;

/// <summary>
/// CONTROLLER — Orchestre la logique d'une target individuelle.
/// Écoute les events de la View, prend les décisions, délègue au manager.
///
/// Côté AR : hit ARUser → notifie NetworkedTargetsManagerAR (qui envoie un Rpc à l'Authority VR).
///           Si VR ne confirme pas dans _arHitResetTimeout secondes, la target revient à son état initial.
/// Côté VR : hit VRBullet → joue l'animation → notifie NetworkedTargetsManagerVR (qui broadcast).
///
/// Compatible Distributed Authority — aucune dépendance NGO directe.
/// </summary>
[RequireComponent(typeof(SingleTargetView))]
public class SingleTargetController : MonoBehaviour
{
    [Header("AR")]
    [Tooltip("Durée (s) avant reset automatique si VR ne confirme pas le hit.")]
    [SerializeField] private float _arHitResetTimeout = 30f;

    private SingleTargetView _view;
    private NetworkedTargetsManagerAR _managerAR;
    private NetworkedTargetsManagerVR _managerVR;

    private bool      _destroyRequested = false;
    private Coroutine _resetCoroutine;

    // ── Initialisation ───────────────────────────────────────────────────────

    /// Appelé par NetworkedTargetsManagerAR au spawn d'une target AR locale.
    public void InitializeAsAR(NetworkedTargetsManagerAR manager)
    {
        _managerAR = manager;
        _view = GetComponent<SingleTargetView>();
        _view.OnARUserHit.AddListener(HandleARUserHit);
    }

    /// Appelé par NetworkedTargetsManagerVR au spawn d'une target VR locale.
    public void InitializeAsVR(NetworkedTargetsManagerVR manager)
    {
        _managerVR = manager;
        _view = GetComponent<SingleTargetView>();
        _view.OnVRBulletHit.AddListener(HandleVRBulletHit);
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void HandleARUserHit()
    {
        if (_destroyRequested) return;
        _destroyRequested = true;

        _view.SetHitColor();
        _managerAR?.NotifyARHit(transform.position);

        // Lance le timeout : si VR ne confirme pas, on remet la target à zéro.
        _resetCoroutine = StartCoroutine(ResetAfterTimeout());
    }

    private IEnumerator ResetAfterTimeout()
    {
        yield return new WaitForSeconds(_arHitResetTimeout);
        _destroyRequested = false;
        _view.ResetColor();
    }

    private void HandleVRBulletHit()
    {
        if (_destroyRequested) return;
        _destroyRequested = true;

        _view.SetHitColor();
        _view.PlayDestroyAnimation(OnVRDestroyAnimComplete);
    }

    private void OnVRDestroyAnimComplete()
    {
        // VR confirme la destruction : on annule le timeout AR si encore actif.
        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
            _resetCoroutine = null;
        }
        _managerVR?.NotifyVRHit(transform.position);
    }

    // ── Nettoyage ────────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        if (_resetCoroutine != null) StopCoroutine(_resetCoroutine);

        if (_view == null) return;
        _view.OnARUserHit.RemoveListener(HandleARUserHit);
        _view.OnVRBulletHit.RemoveListener(HandleVRBulletHit);
    }
}
