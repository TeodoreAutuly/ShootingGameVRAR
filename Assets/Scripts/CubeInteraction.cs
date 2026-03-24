using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(XRSimpleInteractable))]
public class CubeInteraction : MonoBehaviour
{
    [Header("Couleurs")]
    public Color colorDefault = Color.white;
    public Color colorOnHover = Color.cyan;

    [Header("Spawner")]
    public PrefabSpawner prefabSpawner;

    [Header("Tag autorisé pour le trigger physique")]
    public string triggerTag = "Player";

    private Renderer _renderer;
    private Material _material;
    private XRSimpleInteractable _interactable;
    private bool _hasSpawnedThisHover = false;

    void Awake()
    {
        _renderer     = GetComponent<Renderer>();
        _interactable = GetComponent<XRSimpleInteractable>();

        // URP : crée une instance du material pour éviter de modifier le shared material
        _material = _renderer.material;
        SetColor(colorDefault);

        _interactable.hoverEntered.AddListener(OnHoverEnter);
        _interactable.hoverExited.AddListener(OnHoverExit);
    }

    void OnDestroy()
    {
        _interactable.hoverEntered.RemoveListener(OnHoverEnter);
        _interactable.hoverExited.RemoveListener(OnHoverExit);
    }

    // ─── Détection par RAYON ─────────────────────────────────────────────────
    private void OnHoverEnter(HoverEnterEventArgs args) => TriggerAction();

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (_interactable.interactorsHovering.Count == 0)
            ResetCube();
    }

    // ─── Détection PHYSIQUE (traversée) ──────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
            TriggerAction();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggerTag))
            if (_interactable.interactorsHovering.Count == 0)
                ResetCube();
    }

    // ─── Logique commune ──────────────────────────────────────────────────────
    private void TriggerAction()
    {
        SetColor(colorOnHover);

        if (!_hasSpawnedThisHover)
        {
            _hasSpawnedThisHover = true;
            if (prefabSpawner != null)
                prefabSpawner.SpawnPrefabs();
        }
    }

    private void ResetCube()
    {
        SetColor(colorDefault);
        _hasSpawnedThisHover = false;
    }

    // ─── Compatibilité URP + Built-in ────────────────────────────────────────
    private void SetColor(Color color)
    {
        // Essaie URP d'abord, puis Built-in en fallback
        if (_material.HasProperty("_BaseColor"))
            _material.SetColor("_BaseColor", color);
        else if (_material.HasProperty("_Color"))
            _material.SetColor("_Color", color);
    }
}