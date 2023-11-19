using UnityEngine;
using UnityEngine.UI;

public class Selectable : MonoBehaviour
{
    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    [SerializeField] private Image _image;
    [SerializeField] private PartType _partType;
    [SerializeField] private bool _delete;
    private Button _button;

    private void Start()
    {
        if (!_delete)
        {
            _image.sprite = Sprites.GetSprite(_partType);
            _button.onClick.AddListener(() => Selecter.SelectSelectable(_partType));
        }
        else
        {
            _image.sprite = Sprites.GetSprite("Delete");
            _button.onClick.AddListener(() => Selecter.SelectDelete());
        }
    }
}
