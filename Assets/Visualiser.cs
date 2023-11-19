using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

//Singleton
public class Visualiser : MonoBehaviour
{
    private void Awake()
    {
        Grid.OnSizeChange += Visualize;
        Grid.OnContentChange += Visualize;
    }

    [SerializeField] private GameObject _partPrefab;
    [SerializeField] private Transform _partsAndLightsParent;
    [SerializeField] private GameObject _visualGrid;
    [SerializeField] private GameObject _background;

    [Header("Messages")]
    [SerializeField] private TMP_Text _copyGridText;
    [SerializeField] private TMP_Text _copyTableText;
    [SerializeField] private TMP_Text _copyTableErrorText;
    [SerializeField] private TMP_Text _pastedGridText;

    private List<GameObject> _createdGOList = new List<GameObject>();


    private void Start()
    {
        //Set Grid start Size:
        Grid.SetGridSize(new Vector2Int(10,10));
        
        //For quicker testing:
        /*
        Grid.CreateGridFromString("4 6\r\nX  Q1 Q2 X\r\nX  W  W  X\r\nr  R  R  r\r\nX  B  B  X\r\nX  W  W  X\r\nX  L1 L2 X");//"6 8\r\nX  X  Q1 Q2 X  X\r\nX  X  B  B  X  X\r\nX  X  W  W  X  X\r\nX  r  R  R  r  X\r\nr  R  W  W  R  r\r\nX  W  W  B  B  X\r\nX  X  B  B  X  X\r\nX  X  L1 L2 X  X", out string t);
        CopyGrid();
        
        //"4 6\r\nXQ1 Q2   X\r\nX  W  W  X\r\nr  R  R  r\r\nX  B  B  X\r\nX  W  W  X\r\nXL1 L2   X\r\n"
        //"4 6\r\nXQ  1 Q  2   X\r\nX  W  W  X\r\nr  R  R  r\r\nX  B  B  X\r\nX  W  W  X\r\nXL  1 L  2   X\r\n"
        //"4 6\r\nXQ1 Q2   X\r\nX  W  W  X\r\nr  R  R  r\r\nX  B  B  X\r\nX  W  W  X\r\nXL1 L2   X\r\n"
        //"4 6\r\nX  Q1   Q2   X\r\nX  W  W  X\r\nr  R  R  r\r\nX  B  B  X\r\nX  W  W  X\r\nX  Q1   Q2   X\r\n"
        //"4 6\r\nX  Q1   Q2   X\r\nX  W  W  X\r\nr  R  R  r\r\nX  B  B  X\r\nX  W  W  X\r\nX  Q1   Q2   X\r\n"
        //"4 6\r\nX  Q1 Q2 X\r\nX  W  W  X\r\nr  R  R  r\r\nX  B  B  X\r\nX  W  W  X\r\nX  Q1 L2 X\r\n"

        //"4 6\r\nX  Q1 Q2 X\r\nX  W  W  X\r\nr  R  R  r\r\nX  B  B  X\r\nX  W  W  X\r\nX  L1 L2 X"
        //"4 6\r\nX  Q1 Q2 X\r\nX  W  W  X\r\nr  R  R  r\r\nX  B  B  X\r\nX  W  W  X\r\nX  L1 L2 X"
        */
    }

    private void Visualize()
    {
        foreach (GameObject partGO in _createdGOList)
        {
            Destroy(partGO);
        }
        _createdGOList.Clear();

        Selecter.Clickables.Clear();
        for (int y = 0; y < Grid.Size.y; y++)
        {
            for (int x = 0; x < Grid.Size.x; x++)
            {
                Vector2Int gridPosition = new Vector2Int(x, y);
                if (Grid.PartOnThisPosition(gridPosition, out Part part))
                {
                    CreatePartGO(gridPosition, part.Length, part);
                }
                else if(Grid.LightOnThisPosition(gridPosition))
                {
                    CreateLightGO(gridPosition);
                }
            }
        }

        _visualGrid.GetComponent<RectTransform>().sizeDelta = Grid.Size;
        _visualGrid.transform.position = new Vector3(Grid.Size.x / 2f, -Grid.Size.y / 2f, 0);

        _background.GetComponent<RectTransform>().sizeDelta = Grid.Size;
        _background.transform.position = new Vector3(Grid.Size.x / 2f, -Grid.Size.y / 2f, 0);
    }

    private void CreatePartGO(Vector2Int gridPosition, int length, Part part)
    {
        GameObject instance = Instantiate(_partPrefab);
        instance.transform.SetParent(_partsAndLightsParent);

        Vector3 position = GridToWorldPosition(gridPosition);
        position.x += (length - 1) / 2f;
        instance.transform.position = position;

        instance.GetComponent<RectTransform>().sizeDelta = new Vector2(length,1);

        instance.GetComponent<Image>().sprite = Sprites.GetSprite(part.type);

        if (part.hasOnClickBahaviour) 
        { 
            instance.AddComponent<Button>();
            Button button = instance.GetComponent<Button>();
            button.onClick.AddListener(part.OnClickBehaviour);
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(1,1,1,1);
            button.colors = colors;
            Selecter.Clickables.Add(button);
        }
        _createdGOList.Add(instance);
    }

    private void CreateLightGO(Vector2Int gridPosition)
    {
        GameObject instance = Instantiate(_partPrefab);
        instance.transform.SetParent(_partsAndLightsParent);

        Vector3 position = GridToWorldPosition(gridPosition);
        instance.transform.position = position;

        instance.GetComponent<Image>().sprite = Sprites.GetSprite("Light");
        _createdGOList.Add(instance);
    }

    /// <summary>
    /// Returns whether it was succesful
    /// </summary>
    public static Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x + 0.5f, -gridPosition.y - 0.5f, 0);
    }

    /// <summary>
    /// Returns whether it was succesful
    /// </summary>
    public static bool WorldToGridPosition(Vector3 worldPosition, out Vector2Int gridPosition)
    {
        gridPosition = new Vector2Int(-1, -1);
        Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(worldPosition.x), -Mathf.CeilToInt(worldPosition.y));
        if (Grid.IsInGridBounds(gridPos))
        {
            gridPosition = gridPos;
            return true;
        }
        return false;
    }

    #region UI Methods
    public void ClearGrid()
    {
        Grid.Clear();
    }

    public void CopyGrid()
    {
        TextEditor textEditor = new TextEditor();
        textEditor.text = Grid.GetGridString();
        textEditor.SelectAll(); 
        textEditor.Copy();
        Fade.Instance.InOutTextFade(_copyGridText);
    }
    
    public void PasteGrid()
    {
        TextEditor textEditor = new TextEditor();
        textEditor.multiline = true;
        textEditor.Paste();
        Grid.CreateGridFromString(textEditor.text);
        Fade.Instance.InOutTextFade(_pastedGridText);
    }

    public void CopyTable()
    {
        TextEditor textEditor = new TextEditor();
        if(Grid.CreateTable(out string table))
        {
            textEditor.text = table;
            textEditor.SelectAll();
            textEditor.Copy();
            Fade.Instance.InOutTextFade(_copyTableText);
        }
        else
        {
            Fade.Instance.InOutTextFade(_copyTableErrorText);
        }
    }
    #endregion
}
