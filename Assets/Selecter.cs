using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Selecter : MonoBehaviour//, IPointerDownHandler, IPointerMoveHandler
{
    [SerializeField] private GameObject _highlighter;

    private static GameObject _highlighterGO;
    private static Image _highlighterImage;

    private static PartType _selectedPartType;
    private static bool _partTypeIsSelected = false;
    private static bool _deleteIsSelected = false;

    /// <summary>
    /// Vector2Int in Grid space
    /// </summary>
    private static Action<Vector2Int> Place;
 
    private EventSystem _eventSystem;
    private Camera _cam;

    public static List<Button> Clickables = new List<Button>();

    private void Awake()
    {
        _eventSystem = EventSystem.current;
        _cam = Camera.main;
        _highlighterGO = _highlighter;
        _highlighterImage = _highlighter.GetComponent<Image>();
        SetHighlighterImage(null);
    }
    
    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        if (Visualiser.WorldToGridPosition(_cam.ScreenToWorldPoint(mousePos), out Vector2Int gridPosition))
        {
            
            Vector3 worldPos = Visualiser.GridToWorldPosition(gridPosition);
            if (_partTypeIsSelected)
            {
                Part part = PartTypeMethods.ConvertToPart(_selectedPartType);
                
                if (part.Length > 1 && gridPosition.x + part.Length - 1 >= Grid.Size.x)
                {
                    worldPos.x += (part.Length - 1) / 2f - (part.Length - 1);
                }
                else worldPos.x += (part.Length - 1) / 2f;
            }
            _highlighterGO.transform.position = worldPos;
            
            /*
            Vector3 worldPos = Visualiser.GridToWorldPosition(gridPosition);
            if (_partTypeIsSelected)
            {
                worldPos.x += (PartTypeMethods.ConvertToPart(_selectedPartType).Length - 1) / 2f;
            }
            _highlighterGO.transform.position = worldPos;
            */

            if (Input.GetMouseButtonDown(0))
            {
                List<RaycastResult> hits = new List<RaycastResult>();
                PointerEventData eventData = new PointerEventData(_eventSystem);
                eventData.position = mousePos;
                _eventSystem.RaycastAll(eventData, hits);
                foreach(RaycastResult hit in hits)
                {
                    //If the mouse is over an UI element don't place anything; layer 5 is the UI layer
                    if (hit.gameObject.layer == 5) return;
                }
                if(Place != null) Place(gridPosition);
            }
        }
    }
    
    private static void SetHighlighterImage(Sprite sprite)
    {
        if(sprite == null)
        {
            _highlighterImage.sprite = null;
            _highlighterImage.enabled = false;
        }
        else
        {
            _highlighterImage.sprite = sprite;
            _highlighterImage.enabled = true;
            _highlighterGO.transform.localScale = new Vector3(sprite.rect.size.x / 300f, sprite.rect.size.y / 300f, 1);
        }
    }

    public static void SelectSelectable(PartType type)
    {
        if (_partTypeIsSelected && _selectedPartType == type)
        {
            Place = null;
            _deleteIsSelected = false;
            _partTypeIsSelected = false;
            SetHighlighterImage(null);
            ActivateClickables(true);
        }
        else
        {
            _partTypeIsSelected = true;
            _selectedPartType = type;
            _deleteIsSelected = false;
            Place = (Vector2Int v) => { Grid.PlacePart(v, type); };
            SetHighlighterImage(Sprites.GetSprite(type));
            ActivateClickables(false);
        }
    }

    public static void SelectDelete()
    {
        if(_deleteIsSelected)
        {
            Place = null;
            _deleteIsSelected = false;
            _partTypeIsSelected = false;
            SetHighlighterImage(null);
            ActivateClickables(true);
        }
        else
        {
            _deleteIsSelected = true;
            _partTypeIsSelected = false;
            Place = (Vector2Int v) => { Grid.DeletePart(v); };
            SetHighlighterImage(Sprites.GetSprite("Delete"));
            ActivateClickables(false);
        }
    }

    private static void ActivateClickables(bool activate)
    {
        foreach (Button button in Clickables)
        {
            button.interactable = activate;
        }
    }
}
