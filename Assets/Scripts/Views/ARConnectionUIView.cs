using UnityEngine;
using TMPro; // Nécessaire pour TMP_InputField
using UnityEngine.UI; // Nécessaire pour Button
using UnityEngine.Events;
using System;

public class ARConnectionUIView : MonoBehaviour
{
    public event Action<string> OnCodeValidated;

    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button validateButton;

    void Start()
    {
        // On écoute le clic sur le bouton
        Debug.Log("[ AR Lobby View ] Initialisation ! ");
        if (validateButton != null)
            validateButton.onClick.AddListener(SubmitCode);

        // On écoute l'appui sur "Entrée" (EndEdit est déclenché quand on valide sur mobile)
        if (inputField != null)
            inputField.onEndEdit.AddListener(delegate { CheckEnterKey(); });
    }

    private void CheckEnterKey()
    {
         Debug.Log("[ AR Lobby View ] CheckEnterKey"); 

        // Sur mobile, on vérifie si l'utilisateur a validé la saisie
        // if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) 
        // {
        //     SubmitCode();
        // }

        SubmitCode();
    }

    public void SubmitCode()
    {
        string enteredText = inputField.text;

        Debug.Log("[ AR Lobby View ] Code prev : " + enteredText);

        // On vérifie que la zone de texte n'est pas vide
        if (!string.IsNullOrEmpty(enteredText))
        {
            Debug.Log("[ AR Lobby View ] Code validé : " + enteredText);
            
            // On émet l'événement avec le texte
            OnCodeValidated?.Invoke(enteredText);
            
            // Optionnel : Effacer le champ après validation
            // inputField.text = "";
        }
        else
        {
            Debug.LogWarning("La zone de texte est vide !");
        }
    }

    private void OnDestroy()
    {
        // Nettoyage des listeners pour éviter les fuites mémoire
        validateButton.onClick.RemoveListener(SubmitCode);
    }

    /// <summary>
    /// Cache l'intégralité du panneau d'UI de connexion.
    /// Peut être appelée en interne après validation ou par une autre classe.
    /// </summary>
    public void HideUI()
    {
        // Désactive le GameObject auquel ce script est attaché.
        // Puisque tous les éléments d'UI sont des enfants (comme on le voit dans ta hiérarchie),
        // ils disparaîtront tous en même temps.
        this.gameObject.SetActive(false);
    }
}