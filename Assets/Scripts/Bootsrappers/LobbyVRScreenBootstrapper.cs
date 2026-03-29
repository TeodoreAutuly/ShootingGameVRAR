using UnityEngine;

public class CodeDisplayInstaller : MonoBehaviour
{
    [SerializeField] private CodeDisplayView view; 

    private CodeDisplayController controller;

    public CodeDisplayController Controller => controller;

    private void Awake()
    {
        if (view == null)
        {
            Debug.LogError("[CodeDisplayInstaller] La vue n'est pas assignée.", this);
            enabled = false;
            return;
        }

        controller = new CodeDisplayController(
            view,
            new AlphanumericCodeGenerator(),
            codeLength: 6,
            timerDurationSeconds: 60f
        );
    }

    private void Update()
    {
        controller?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        controller?.Dispose();
    }
}