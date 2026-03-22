using System;

public interface ICodeDisplayView
{
    event Action GenerateButtonClicked;

    void SetCodeText(string value);
    void SetTimerText(string value);
    void SetGenerateButtonInteractable(bool interactable);
}