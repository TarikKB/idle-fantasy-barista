using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GCCell : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;


    // copiesNeeded of -1 means max level
    public void Setup(GCData data, Color boxColor, int level, int copies = 0, int copiesNeeded = -1)
    {
        bool owned = level > 0;
        backgroundImage.color = boxColor;
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