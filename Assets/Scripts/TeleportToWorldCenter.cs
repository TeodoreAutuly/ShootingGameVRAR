using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportToWorldCenterOnLeftGrip : MonoBehaviour
{
    [Header("XR Origin Root")]
    [SerializeField] private Transform xrOriginRoot;

    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference leftGripAction;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = false;

    private void Awake()
    {
        if (xrOriginRoot == null)
            xrOriginRoot = transform;
    }

    private void OnEnable()
    {
        if (leftGripAction == null || leftGripAction.action == null)
        {
            Debug.LogWarning("[TeleportToWorldCenterOnLeftGrip] Left Grip Action is not assigned.", this);
            return;
        }

        leftGripAction.action.performed += OnLeftGripPerformed;
        leftGripAction.action.Enable();
    }

    private void OnDisable()
    {
        if (leftGripAction == null || leftGripAction.action == null)
            return;

        leftGripAction.action.performed -= OnLeftGripPerformed;
        leftGripAction.action.Disable();
    }

    private void OnLeftGripPerformed(InputAction.CallbackContext context)
    {
        TeleportToWorldCenter();
    }

    private void TeleportToWorldCenter()
    {
        if (xrOriginRoot == null)
        {
            Debug.LogWarning("[TeleportToWorldCenterOnLeftGrip] XR Origin Root is null.", this);
            return;
        }

        Vector3 currentPosition = xrOriginRoot.position;
        currentPosition.x = 0f;
        currentPosition.z = 0f;

        xrOriginRoot.position = currentPosition;

        if (enableLogs)
        {
            Debug.Log(
                $"[TeleportToWorldCenterOnLeftGrip] Player moved to world center. New position = {xrOriginRoot.position}",
                this
            );
        }
    }
}