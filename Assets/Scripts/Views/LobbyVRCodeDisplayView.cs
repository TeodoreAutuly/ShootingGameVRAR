using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeDisplayView : MonoBehaviour, ICodeDisplayView
{
    [Header("Worldspace UI")]
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Button generateButton;

    public event Action GenerateButtonClicked;

    private void Awake()
    {
        if (generateButton != null)
        {
            generateButton.onClick.AddListener(HandleGenerateClicked);
        }
    }

    private void Start()
    {
        SetCodeText("------");
        SetTimerText("60");
    }

    private void OnDestroy()
    {
        if (generateButton != null)
        {
            generateButton.onClick.RemoveListener(HandleGenerateClicked);
        }
    }

    private void HandleGenerateClicked()
    {
        GenerateButtonClicked?.Invoke();
    }

    public void SetCodeText(string value)
    {
        if (codeText != null)
        {
            codeText.text = value;
        }
    }

    public void SetTimerText(string value)
    {
        if (timerText != null)
        {
            timerText.text = value;
        }
    }

    public void SetGenerateButtonInteractable(bool interactable)
    {
        if (generateButton != null)
        {
            generateButton.interactable = interactable;
        }
    }

    public void HideLobby()
    {
        // Désactive le GameObject auquel ce script est attaché.
        // Puisque tous les éléments d'UI sont des enfants (comme on le voit dans ta hiérarchie),
        // ils disparaîtront tous en même temps.
        this.gameObject.SetActive(false);
    }
}