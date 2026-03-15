using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ARDroneController : NetworkBehaviour
{
    [Header("Mouvement & Rotation")]
    [Tooltip("Vitesse de déplacement horizontale du drone.")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("La caméra AR utilisée par le joueur (assignée depuis le prefab).")]
    [SerializeField] private Transform arCameraTransform;
    
    [Header("Input (UI)")]
    // References to your Input System Actions (e.g. On-Screen Stick)
    [SerializeField] private InputActionReference moveInput;

    private void Update()
    {
        // Seules les actions du Propriétaire (Le client sur Android) doivent être gérées.
        if (!IsOwner) return;

        HandleMovement();
        HandleRotationSync();
    }

    private void HandleMovement()
    {
        if (moveInput == null) return;
        
        Vector2 inputDir = moveInput.action.ReadValue<Vector2>();
        
        // Mouvement relatif à la direction de la caméra (pour que 'Avance' soit toujours 'Devant le joueur AR')
        if (arCameraTransform != null && inputDir.sqrMagnitude > 0)
        {
            Vector3 camForward = arCameraTransform.forward;
            Vector3 camRight = arCameraTransform.right;

            // Annuler l'axe Y pour un mouvement horizontal plat
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * inputDir.y + camRight * inputDir.x).normalized;

            // Déplacement physique du NetworkObject (Il sera synchronisé automatiquement si NetworkTransform est attaché)
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    private void HandleRotationSync()
    {
        if (arCameraTransform != null)
        {
            // On veut que le corps 3D du drone regarde dans la même direction que la caméra du téléphone AR, 
            // en ignorant l'inclinaison de haut en bas (Pitch) pour qu'il reste à plat.
            float yRotation = arCameraTransform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }
}