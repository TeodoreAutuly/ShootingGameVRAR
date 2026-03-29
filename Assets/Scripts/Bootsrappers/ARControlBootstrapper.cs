using UnityEngine;

public class ARControlBootstrapper : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private ARControlView view;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform arCamera;

    [Header("Settings")]
    [SerializeField] private float speed = 1.5f;

    private ARControlController _controller;

    private void Start()
    {
        if (view == null || xrOrigin == null || arCamera == null)
        {
            Debug.LogError("[ARControlBootstrapper] Références manquantes.", this);
            return;
        }

        var locomotion = new ARLocomotionService(xrOrigin, arCamera, speed);
        _controller = new ARControlController(locomotion);
        _controller.Subscribe(view);

        Debug.Log("[ARControlBootstrapper] ARControl initialisé.", this);
    }

    private void Update()
    {
        _controller?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (_controller != null && view != null)
            _controller.Unsubscribe(view);
    }
}