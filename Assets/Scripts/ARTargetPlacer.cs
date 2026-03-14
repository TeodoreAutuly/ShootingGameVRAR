using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARTargetPlacer : MonoBehaviour
{
    [SerializeField] private ARRaycastManager _raycastManager;
    [SerializeField] private GameObject _targetPrefab;

    private GameObject _spawnedTarget;
    private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        // Vérifie qu'il y a exactement un doigt sur l'écran
        if (Touch.activeTouches.Count == 0) return;

        Touch touch = Touch.activeTouches[0];

        // Détecte uniquement le moment où le doigt touche l'écran
        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;

        Vector2 screenPos = touch.screenPosition;

        Debug.Log($"Touch détecté à : {screenPos}, Résolution : {Screen.width}x{Screen.height}");

        if (_raycastManager.Raycast(screenPos, _hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = _hits[0].pose;

            if (_spawnedTarget == null)
                _spawnedTarget = Instantiate(_targetPrefab, hitPose.position, hitPose.rotation);
            else
                _spawnedTarget.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
        }
        else
        {
            Debug.Log("Aucun plan détecté à cet endroit");
        }
    }
}