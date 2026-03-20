using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(XRSimpleInteractable))]
public class PrefabVisibilityToggle : MonoBehaviour
{
    [Header("Tags")]
    public string tagVisible = "View";
    public string tagHidden  = "Hide";

    [Header("Tag physique du joueur")]
    public string playerTag = "Player";

    private Renderer _renderer;
    private XRSimpleInteractable _interactable;
    private bool _hasToggledThisInteraction = false;

    void Awake()
    {
        _renderer     = GetComponent<Renderer>();
        _interactable = GetComponent<XRSimpleInteractable>();

        // Applique l'état initial selon le tag assigné sur le prefab
        ApplyCurrentState();

        // Abonnement aux events XR (rayon)
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
            TryToggle();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            _hasToggledThisInteraction = false;
    }

    // ─── Logique commune ──────────────────────────────────────────────────────
    private void TryToggle()
    {
        if (_hasToggledThisInteraction) return;
        _hasToggledThisInteraction = true;

        if (gameObject.CompareTag(tagVisible))
        {
            // View → Hide
            gameObject.tag = tagHidden;
            _renderer.enabled = false;
            Debug.Log($"{gameObject.name} : View → Hide");
        }
        else if (gameObject.CompareTag(tagHidden))
        {
            // Hide → View
            gameObject.tag = tagVisible;
            _renderer.enabled = true;
            Debug.Log($"{gameObject.name} : Hide → View");
        }
    }

    // Applique l'état visuel selon le tag actuel
    private void ApplyCurrentState()
    {
        if (gameObject.CompareTag(tagVisible))
            _renderer.enabled = true;
        else if (gameObject.CompareTag(tagHidden))
            _renderer.enabled = false;
    }
}