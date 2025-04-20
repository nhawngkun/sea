using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoatUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject drivePromptPanel;
    public GameObject exitPromptPanel;
    public TextMeshProUGUI drivePromptText;
    public TextMeshProUGUI exitPromptText;

    [Header("Key Settings")]
    public KeyCode interactionKey = KeyCode.F;

    void Start()
    {
        // Initialize UI
        if (drivePromptPanel != null)
            drivePromptPanel.SetActive(false);

        if (exitPromptPanel != null)
            exitPromptPanel.SetActive(false);

        // Update prompt texts with the correct key
        UpdatePromptTexts();
    }

    // Update the prompt texts with the correct key
    void UpdatePromptTexts()
    {
        if (drivePromptText != null)
            drivePromptText.text = $"Press [{interactionKey}] to Drive Boat";

        if (exitPromptText != null)
            exitPromptText.text = $"Press [{interactionKey}] to Exit Boat";
    }

    // Show drive prompt
    public void ShowDrivePrompt(bool show)
    {
        if (drivePromptPanel != null)
            drivePromptPanel.SetActive(show);
    }

    // Show exit prompt
    public void ShowExitPrompt(bool show)
    {
        if (exitPromptPanel != null)
            exitPromptPanel.SetActive(show);
    }

    // Hide all prompts
    public void HideAllPrompts()
    {
        ShowDrivePrompt(false);
        ShowExitPrompt(false);
    }
}