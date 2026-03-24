using UnityEngine;

/// <summary>
/// CONTROLLER — Orchestre la logique d'une target individuelle.
/// Écoute les events de la View, prend les décisions, délègue au manager.
///
/// Côté AR : hit ARUser → notifie NetworkedTargetsManagerAR (qui envoie un Rpc à l'Authority VR).
/// Côté VR : hit VRBullet → joue l'animation → notifie NetworkedTargetsManagerVR (qui broadcast).
///
/// Compatible Distributed Authority — aucune dépendance NGO directe.
/// </summary>
[RequireComponent(typeof(SingleTargetView))]
public class SingleTargetController : MonoBehaviour
{
    private SingleTargetView _view;
    private NetworkedTargetsManagerAR _managerAR;
    private NetworkedTargetsManagerVR _managerVR;

    private bool _destroyRequested = false;

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

        // Notifie le manager AR : il détruira cette target locale
        // et enverra un Rpc à l'Authority (VR) pour spawner côté VR.
        _managerAR?.NotifyARHit(transform.position);
    }

    private void HandleVRBulletHit()
    {
        if (_destroyRequested) return;
        _destroyRequested = true;

        _view.SetHitColor();

        // Joue l'animation avant de notifier — destruction après l'anim.
        _view.PlayDestroyAnimation(OnVRDestroyAnimComplete);
    }

    private void OnVRDestroyAnimComplete()
    {
        // Notifie le manager VR : il détruira localement et broadcastera vers AR.
        _managerVR?.NotifyVRHit(transform.position);
    }

    // ── Nettoyage ────────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        if (_view == null) return;
        _view.OnARUserHit.RemoveListener(HandleARUserHit);
        _view.OnVRBulletHit.RemoveListener(HandleVRBulletHit);
    }
}