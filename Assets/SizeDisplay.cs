using UnityEngine;
using TMPro;

public class SizeDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private RectTransform _rectTransform;

    private void Awake()
    {
        Grid.OnSizeChange += UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        _text.text = "Size: " + Grid.Size.x + ", " + Grid.Size.y;
        //I am not using a monospace font and thus this scaling will always be off
        _rectTransform.sizeDelta = new Vector2(_text.preferredWidth + 20, _rectTransform.sizeDelta.y);//(Grid.Size.x / 10 + Grid.Size.y / 10 + 8) * 40, _rectTransform.sizeDelta.y);
    }
}
