using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// VIEW — Affiche et anime un prefab target.
/// AR  : détection via XRBaseInteractable.selectEntered uniquement.
/// VR  : détection via OnTriggerEnter (tag "VRBullet") uniquement.
/// Pas de logique réseau. Délègue via UnityEvents au Controller.
/// Compatible Distributed Authority — aucune dépendance NGO.
/// </summary>
public class SingleTargetView : MonoBehaviour
{
    public enum Side { AR, VR }

    [Header("Côté")]
    [Tooltip("AR : réagit à selectEntered (XR). VR : réagit au tag VRBullet (trigger physique).")]
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
        if (_side != Side.AR) return;

        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelectEntered);
        else
            Debug.LogWarning($"[SingleTargetView] Aucun XRBaseInteractable trouvé sur {gameObject.name}.", this);
    }

    private void OnDestroy()
    {
        if (_side != Side.AR) return;

        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    // ── Handler XR Select (AR uniquement) ────────────────────────────────────

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        OnARUserHit.Invoke();
    }

    // ── Détection de collision (VR uniquement) ────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (_side == Side.VR && other.CompareTag("VRBullet")){
            Debug.Log("[VR] Touché !");
            OnVRBulletHit.Invoke();    
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

    // ── Helpers ──────────────────────────────────────────────────────────────

    private IEnumerator AnimateThenCallback(UnityAction onComplete)
    {
        _animator.SetTrigger(_destroyAnimTrigger);
        // Deux frames : une pour que le trigger soit pris en compte,
        // une pour que la transition d'état soit effectuée.
        yield return null;
        yield return null;
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);
        onComplete?.Invoke();
    }
}
