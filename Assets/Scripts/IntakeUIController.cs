using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IntakeUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject intakePanel;
    [SerializeField] private GameObject tutorPanel;

    [Header("Inputs")]
    [SerializeField] private TMP_Dropdown educationDropdown;
    [SerializeField] private TMP_Dropdown lastMathDropdown;

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text errorText;

    [Header("Tutor")]
    [SerializeField] private EquationController equationController;

    // Store selections for later (telemetry/session)
    public string EducationLevel { get; private set; } = "";
    public string LastMathClass { get; private set; } = "";

    private const string Placeholder = "Select...";

    private void Awake()
    {
        SetupDropdown(educationDropdown, new List<string>
        {
            Placeholder,
            "High school or GED",
            "Some college (no degree)",
            "Associate degree",
            "Bachelor’s degree",
            "Graduate degree",
            "Other"
        });

        SetupDropdown(lastMathDropdown, new List<string>
        {
            Placeholder,
            "Within the last year",
            "1–3 years ago",
            "3+ years ago",
            "Currently enrolled"
        });

        if (errorText != null) errorText.text = "";

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartClicked);

        // Ensure correct initial visibility
        if (intakePanel != null) intakePanel.SetActive(true);
        if (tutorPanel != null) tutorPanel.SetActive(false);
    }

    private void SetupDropdown(TMP_Dropdown dd, List<string> options)
    {
        dd.ClearOptions();
        dd.AddOptions(options);
        dd.value = 0;
        dd.RefreshShownValue();
    }

    private void OnStartClicked()
    {
        if (educationDropdown.value == 0 || lastMathDropdown.value == 0)
        {
            if (errorText != null) errorText.text = "Please complete both fields.";
            return;
        }

        EducationLevel = educationDropdown.options[educationDropdown.value].text;
        LastMathClass = lastMathDropdown.options[lastMathDropdown.value].text;

        if (errorText != null) errorText.text = "";

        // Swap panels
        if (intakePanel != null) intakePanel.SetActive(false);
        if (tutorPanel != null) tutorPanel.SetActive(true);

        // Start the tutor session
        if (equationController != null)
        {
            equationController.BeginSession(EducationLevel, LastMathClass);
        }
        else
        {
           // Debug.LogError("IntakeUIController: EquationController reference not set in Inspector.");
        }
    }
}