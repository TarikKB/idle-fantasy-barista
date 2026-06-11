using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GCCell : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;


    public void Setup(GCData data, Color boxColor, int level)
    {   
        bool owned = level > 0;
        backgroundImage.color = boxColor;
        if (data != null && nameText != null)
        {
            nameText.text = owned
                ? $"Level {level} : {data.characterName}"
                : "???";  
        }
    }
}