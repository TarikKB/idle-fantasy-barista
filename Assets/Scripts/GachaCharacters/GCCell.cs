using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GCCell : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;


    public void Setup(GCData data, Color boxColor, int level)
    {   
        bool owned = level > 0;
        backgroundImage.color = boxColor;
        if (data != null && nameText != null)
        {
            nameText.text = owned ? data.characterName : "???";
            levelText.text = owned ? $"Level {level}" : "???";

            // nameText.fontSize
        }
    }
}