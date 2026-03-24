using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// VIEW — Affiche et anime un prefab target.
/// Deux variantes selon le côté : AR (écoute "ARUser") ou VR (écoute "VRBullet").
/// Pas de logique réseau. Délègue via UnityEvents au Controller.
/// Compatible Distributed Authority — aucune dépendance NGO.
/// </summary>
public class SingleTargetView : MonoBehaviour
{
    public enum Side { AR, VR }

    [Header("Côté")]
    [Tooltip("AR : réagit au tag ARUser et XR Hover. VR : réagit au tag VRBullet.")]
    [SerializeField] private Side _side = Side.AR;

    [Header("Références visuelles")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Animator _animator;

    [Header("Couleurs")]
    [SerializeField] private Color _defaultColor = Color.white;
    [SerializeField] private Color _hitColor     = Color.red;

    [Header("Animation")]
    [SerializeField] private string _destroyAnimTrigger = "Destroy";

    // ── Events exposés au Controller ──────────────────────────────────────────
    [HideInInspector] public UnityEvent OnARUserHit   = new();
    [HideInInspector] public UnityEvent OnVRBulletHit = new();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Abonnement automatique au XR Interactable si présent sur le GO
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            Debug.Log($"[SingleTargetView] Abonné au hoverEntered du XRBaseInteractable ({_side}).", this);
        }
        else
        {
            Debug.LogWarning($"[SingleTargetView] Aucun XRBaseInteractable trouvé sur {gameObject.name}.", this);
        }
    }

    private void OnDestroy()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
    }

    // ── Handler XR Hover ─────────────────────────────────────────────────────

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log($"[SingleTargetView] HoverEntered détecté sur {gameObject.name} (Side={_side}).", this);

        switch (_side)
        {
            case Side.AR:
                OnARUserHit.Invoke();
                break;
            case Side.VR:
                OnVRBulletHit.Invoke();
                break;
        }
    }

    // ── API publique ──────────────────────────────────────────────────────────

    public void SetActive(bool active) => gameObject.SetActive(active);

    public void PlayDestroyAnimation(UnityAction onComplete)
    {
        if (_animator != null)
            StartCoroutine(AnimateThenCallback(onComplete));
        else
            onComplete?.Invoke();
    }

    public void SetHitColor()
    {
        if (_renderer != null)
            _renderer.material.color = _hitColor;
    }

    public void ResetColor()
    {
        if (_renderer != null)
            _renderer.material.color = _defaultColor;
    }

    // ── Détection de collision (alternative au XR Interactable) ──────────────

    private void OnTriggerEnter(Collider other)
    {
        switch (_side)
        {
            case Side.AR when other.CompareTag("ARUser"):
                OnARUserHit.Invoke();
                break;
            case Side.VR when other.CompareTag("VRBullet"):
                OnVRBulletHit.Invoke();
                break;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator AnimateThenCallback(UnityAction onComplete)
    {
        _animator.SetTrigger(_destroyAnimTrigger);
        yield return null;
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);
        onComplete?.Invoke();
    }
}