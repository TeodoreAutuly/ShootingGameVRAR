using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class GunController : MonoBehaviour
{
    [Header("Réglages")]
    public int maxAmmo = 15;
    private int currentAmmo;

    [Header("Bouton de Rechargement")]
    public InputActionReference reloadButton; // La touche pour recharger

    [Header("Événements à lier")]
    public UnityEvent OnFireAllowed; // Ce qui se passe quand on a le droit de tirer
    public UnityEvent OnEmptyClick;  // Ce qui se passe si on tire à vide

    private void Start()
    {
        currentAmmo = maxAmmo; // On commence avec un chargeur plein
    }

    private void OnEnable()
    {
        if (reloadButton != null)
            reloadButton.action.performed += ReloadGun;
    }

    private void OnDisable()
    {
        if (reloadButton != null)
            reloadButton.action.performed -= ReloadGun;
    }

    public void TryToFire()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            Debug.Log("Tir ! Balles restantes : " + currentAmmo);
            OnFireAllowed.Invoke(); 
        }
        else
        {
            Debug.Log("Clic ! Plus de balles.");
            OnEmptyClick.Invoke(); 
        }
    }

    // Fonction appelée par le bouton
    private void ReloadGun(InputAction.CallbackContext context)
    {
        currentAmmo = maxAmmo;
        Debug.Log("Rechargement ! Balles : " + currentAmmo);
    }
}