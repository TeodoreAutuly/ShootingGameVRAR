using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class CubeInteraction : MonoBehaviour
{
    [Header("Couleurs")]
    public Color colorDefault = Color.white;
    public Color colorOnHover = Color.cyan;

    [Header("Spawner")]
    public PrefabSpawner prefabSpawner;

    private Renderer _renderer;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable _interactable;
    private bool _hasSpawnedThisHover = false;

    // Tags des objets physiques autorisés à déclencher l'action
    // (évite que n'importe quel objet dans la scène déclenche le trigger)
    [Header("Tag autorisé pour le trigger physique")]
    public string triggerTag = "Player";

    void Awake()
    {
        _renderer     = GetComponent<Renderer>();
        _interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        _renderer.material.color = colorDefault;

        // Abonnement aux events XR (rayon)
        _interactable.hoverEntered.AddListener(OnHoverEnter);
        _interactable.hoverExited.AddListener(OnHoverExit);
    }

    void OnDestroy()
    {
        _interactable.hoverEntered.RemoveListener(OnHoverEnter);
        _interactable.hoverExited.RemoveListener(OnHoverExit);
    }

    // ─── Détection par RAYON (Ray Interactor) ────────────────────────────────

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        TriggerAction();
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (_interactable.interactorsHovering.Count == 0)
        {
            ResetCube();
        }
    }

    // ─── Détection PHYSIQUE (contact / traversée) ────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            TriggerAction();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            // Ne remet la couleur que si le rayon ne hover pas non plus
            if (_interactable.interactorsHovering.Count == 0)
            {
                ResetCube();
            }
        }
    }

    // ─── Logique commune ─────────────────────────────────────────────────────

    private void TriggerAction()
    {
        _renderer.material.color = colorOnHover;

        if (!_hasSpawnedThisHover)
        {
            _hasSpawnedThisHover = true;
            if (prefabSpawner != null)
                prefabSpawner.SpawnPrefabs();
        }
    }

    private void ResetCube()
    {
        _renderer.material.color = colorDefault;
        _hasSpawnedThisHover = false;
    }
}
