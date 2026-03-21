using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class PrefabVisibilityToggle : MonoBehaviour
{
    [Header("Tag physique du joueur")]
    public string playerTag = "Player";

    private XRSimpleInteractable _interactable;
    private bool _hasToggledThisInteraction = false;
    private bool _isVisible = true;

    // Tous les renderers y compris les enfants
    private Renderer[] _renderers;

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();

        // Récupère TOUS les renderers (parent + enfants)
        _renderers = GetComponentsInChildren<Renderer>();

        _isVisible = true;
        SetVisibility(true);

        _interactable.hoverEntered.AddListener(OnHoverEnter);
        _interactable.hoverExited.AddListener(OnHoverExit);
    }

    void OnDestroy()
    {
        _interactable.hoverEntered.RemoveListener(OnHoverEnter);
        _interactable.hoverExited.RemoveListener(OnHoverExit);
    }

    // ─── Rayon ───────────────────────────────────────────────────────────────
    private void OnHoverEnter(HoverEnterEventArgs args) => TryToggle();

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (_interactable.interactorsHovering.Count == 0)
            _hasToggledThisInteraction = false;
    }

    // ─── Traversée physique ───────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"TriggerEnter → {gameObject.name} | visible={_isVisible}");
            TryToggle();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Toujours reset au exit → permet de retraverser dans les 2 sens
            _hasToggledThisInteraction = false;
            Debug.Log($"TriggerExit → {gameObject.name} | visible={_isVisible}");
        }
    }

    // ─── Logique commune ──────────────────────────────────────────────────────
    private void TryToggle()
    {
        if (_hasToggledThisInteraction) return;
        _hasToggledThisInteraction = true;

        _isVisible = !_isVisible;
        SetVisibility(_isVisible);

        Debug.Log($"{gameObject.name} → {(_isVisible ? "VISIBLE ✅" : "INVISIBLE ❌")}");
    }

    // ─── Active/désactive tous les renderers ──────────────────────────────────
    private void SetVisibility(bool visible)
    {
        foreach (Renderer r in _renderers)
            r.enabled = visible;
    }
}