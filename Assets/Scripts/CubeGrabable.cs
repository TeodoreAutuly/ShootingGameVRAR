using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Renderer))]
public class HoverColorChanger : NetworkBehaviour
{
    [Header("Couleurs")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    private XRGrabInteractable grabInteractable;
    private Renderer objectRenderer;
    private Material objectMaterial;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        objectRenderer = GetComponent<Renderer>();

        objectMaterial = objectRenderer.material;
        ApplyColor(normalColor);
    }

    private void OnEnable()
    {
        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);
    }

    private void OnDisable()
    {
        grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
        grabInteractable.hoverExited.RemoveListener(OnHoverExited);
    }

    public override void OnNetworkSpawn()
    {
        ApplyColor(normalColor);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        SetHoverStateRpc(true);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        SetHoverStateRpc(false);
    }

    [Rpc(SendTo.Everyone)]
    private void SetHoverStateRpc(bool isHovered)
    {
        ApplyColor(isHovered ? hoverColor : normalColor);
    }

    private void ApplyColor(Color color)
    {
        if (objectMaterial == null)
            return;

        objectMaterial.color = color;
    }
}