using System;
using UnityEngine;

public class SharedTargetView : MonoBehaviour
{
    [Header("Visuel")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color touchedColor = Color.green;

    [Header("Tags")]
    [SerializeField] private string bulletTag = "XRBullet";
    [SerializeField] private string arPlayerTag = "ARPlayer";

    public event Action OnTouchedByARPlayer;
    public event Action OnHitByBullet;

    private Color _initialColor;

    private void Awake()
    {
        if (targetRenderer != null)
            _initialColor = targetRenderer.material.color;
    }

    public void SetLayer(int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in transform)
            child.gameObject.layer = layer;
    }

    public void SetPosition(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }

    public Vector3 GetPosition() => transform.position;

    public void ApplyTouchedVisual()
    {
        if (targetRenderer != null)
            targetRenderer.material.color = touchedColor;
    }

    public void ResetVisual()
    {
        if (targetRenderer != null)
            targetRenderer.material.color = _initialColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bulletTag))
        {
            OnHitByBullet?.Invoke();
            return;
        }

        if (other.CompareTag(arPlayerTag))
        {
            OnTouchedByARPlayer?.Invoke();
        }
    }
}