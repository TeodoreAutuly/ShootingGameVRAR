using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ARControlView : MonoBehaviour
{
    [SerializeField] private Button buttonUp;
    [SerializeField] private Button buttonDown;
    [SerializeField] private Button buttonForward;
    [SerializeField] private Button buttonBackward;

    public event Action OnUpPressed;
    public event Action OnUpReleased;
    public event Action OnDownPressed;
    public event Action OnDownReleased;
    public event Action OnForwardPressed;
    public event Action OnForwardReleased;
    public event Action OnBackwardPressed;
    public event Action OnBackwardReleased;

    private void Awake()
    {
        Register(buttonUp,       () => OnUpPressed?.Invoke(),       () => OnUpReleased?.Invoke());
        Register(buttonDown,     () => OnDownPressed?.Invoke(),     () => OnDownReleased?.Invoke());
        Register(buttonForward,  () => OnForwardPressed?.Invoke(),  () => OnForwardReleased?.Invoke());
        Register(buttonBackward, () => OnBackwardPressed?.Invoke(), () => OnBackwardReleased?.Invoke());
    }

    private void Register(Button button, Action onPress, Action onRelease)
    {
        if (button == null)
        {
            Debug.LogWarning($"[ARControlView] Un bouton n'est pas assigné.", this);
            return;
        }

        var trigger = button.gameObject.AddComponent<EventTrigger>();

        var pressEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pressEntry.callback.AddListener(_ => onPress());
        trigger.triggers.Add(pressEntry);

        var releaseEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        releaseEntry.callback.AddListener(_ => onRelease());
        trigger.triggers.Add(releaseEntry);
    }
}