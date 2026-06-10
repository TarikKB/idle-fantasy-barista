using UnityEngine;
using UnityEngine.UI;

public class GCCell : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;

    public void Setup(GCData data, Color boxColor)
    {
        backgroundImage.color = boxColor;
    }
}