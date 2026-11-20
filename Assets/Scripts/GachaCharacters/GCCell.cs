using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GCCell : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI buffText;
    [SerializeField] private UnityEngine.UI.Button assignmentButton;
    [SerializeField] private TextMeshProUGUI assignmentText;
    private GCRosterManager roster;
    private GCData character;

    public void SetupAssignment(GCData data, GCRosterManager employeeRoster, bool allowAssignment)
    {
        character = data;
        roster = employeeRoster;
        bool owned = roster != null && roster.GetLevel(data) > 0;
        if (buffText != null)
        {
            int previewLevel = owned ? roster.GetLevel(data) : 1;
            buffText.text = data.BuffName + "\n" + data.GetEffectDescription(previewLevel);
        }
        if (assignmentButton == null) return;
        assignmentButton.gameObject.SetActive(allowAssignment);
        assignmentButton.onClick.RemoveListener(ToggleAssignment);
        assignmentButton.onClick.AddListener(ToggleAssignment);
        bool assigned = roster != null && roster.IsAssigned(data);
        bool hasSpace = false;
        if (roster != null)
            for (int i = 0; i < GCRosterManager.EmployeeSlotCount; i++)
                if (roster.GetAssignedEmployee(i) == null) hasSpace = true;
        assignmentButton.interactable = owned && (assigned || hasSpace);
        if (assignmentText != null) assignmentText.text = !owned ? "Not owned" : assigned ? "Remove" : hasSpace ? "Assign" : "Team full";
    }

    private void ToggleAssignment()
    {
        if (roster != null) roster.TryToggleAssignment(character);
    }

    // copiesNeededof -1 means max level
    public void Setup(GCData data, Color boxColor, int level, int copies = 0, int copiesNeeded = -1)
    {
        bool owned = level > 0;
        backgroundImage.color = boxColor;
        float luminance = boxColor.r * 0.2126f + boxColor.g * 0.7152f + boxColor.b * 0.0722f;
        Color textColor = luminance < 0.35f ? new Color(1f, 0.93f, 0.78f) : Color.black;
        if (nameText != null) nameText.color = textColor;
        if (levelText != null) levelText.color = textColor;
        if (buffText != null) buffText.color = textColor;
        if (data != null && nameText != null)
        {
            nameText.text = owned ? data.characterName : "???";
            if (!owned)
                levelText.text = "???";
            else if (copiesNeeded > 0)
                levelText.text = $"Level {level} ({copies}/{copiesNeeded})";
            else
                levelText.text = $"Level {level} (MAX)";

            // nameText.fontSize
        }
    }
}