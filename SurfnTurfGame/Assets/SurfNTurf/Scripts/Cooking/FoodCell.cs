using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.VFX;
using TMPro;

public class FoodCell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public int cellRotation;
    public GameObject cellVisual;
    public bool onGrid;
    public int cellID;
    public bool found;
    public float cellScale;

    public int cellTexturePosition;
    public Vector2Int textureGridSize;

    public List<FoodCell> groupCells;
    public List<FoodCell> neighborCells;

    private VisualEffect vfx;
    private CellData cellData;

    public int bakedStage;
    public bool burned;

    private Vector2Int[] groupOffsets = new Vector2Int[]
    {
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(-1,  0), new Vector2Int(1,  0),
        new Vector2Int(-1,  1), new Vector2Int(0,  1), new Vector2Int(1,  1),
    };

    Vector2Int[] neighborOffsets = {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1)
        };

    public void GenerateFoodCell(Vector2Int _gridPosition, Vector2 _worldPosition, Transform _parent, bool _onGrid, int cellID, float cellScale, int _cellTexturePosition, Vector2Int _textureGridSize)
    {
        groupCells = new List<FoodCell>();
        neighborCells = new List<FoodCell>();

        SetParent(_parent, _onGrid);
        SetPosition(_gridPosition, _worldPosition);
        SetCellData(cellID);
        GenerateVisual(cellScale, _cellTexturePosition, _textureGridSize);
    }

    public void SetParent(Transform _parent, bool _onGrid)
    {
        transform.parent = _parent;
        onGrid = _onGrid;
    }

    public void SetPosition(Vector2Int _gridPosition, Vector2 _worldPosition)
    {
        transform.localPosition = new Vector3(_worldPosition.x, _worldPosition.y, 0);
        transform.localRotation = Quaternion.identity;
        gridPosition = _gridPosition;
        transform.name = gridPosition.ToString();
    }

    public void SetCellData(int _cellID)
    {
        cellID = _cellID;
        cellData = BlackBoard.cookingDatabase.GetCellData(cellID);
        bakedStage = cellData.maxBakedStage;
    }

    public void SetRotation(int _cellRotation)
    {
        cellRotation = _cellRotation;
    }

    public void SetGroup(List<FoodCell> _groupCells)
    {
        groupCells.Clear();
        foreach (FoodCell cell in _groupCells)
        {
            if(cell.gridPosition != gridPosition)
            {
                foreach (Vector2Int offset in groupOffsets)
                {
                    if(gridPosition + offset == cell.gridPosition)
                    {
                        groupCells.Add(cell);
                    }
                }
            }
        }
    }

    public void SetNeighbors(List<FoodCell> _grid)
    {

        neighborCells.Clear();

        foreach (Vector2Int offset in neighborOffsets)
        {
            FoodCell neighborCell = _grid.Find(cell => cell.gridPosition == gridPosition + offset);

            if (neighborCell != null)
            {
                neighborCells.Add(neighborCell);
            }
        }
    }

    public void Bake()
    {
        bakedStage--;
        TextMeshProUGUI text = cellVisual.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        text.text = bakedStage.ToString();
        if(bakedStage <= 0)
        {
            text.color = Color.black;
            burned = true;
        }
    }

    public void PlayEffect(string type)
    {
        vfx.SendEvent(type);
    }

    public void GenerateVisual(float _cellScale, int _cellTexturePosition, Vector2Int _textureGridSize)
    {
        cellScale = _cellScale;
        cellVisual = Instantiate(cellVisual, transform);
        cellVisual.transform.localPosition += new Vector3(0, 0, -0.01f);
        cellVisual.transform.localScale = Vector3.one * cellScale * 1.02f;
        //cellVisual.GetComponent<SpriteRenderer>().color = BlackBoard.cookingDatabase.GetCellData(cellID).color;

        cellTexturePosition = _cellTexturePosition;
        textureGridSize = _textureGridSize;

        vfx = cellVisual.GetComponent<VisualEffect>();

        TextMeshProUGUI text = cellVisual.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        text.text = bakedStage.ToString();

        //cellMaterial.SetTexture("_BaseMap", BlackBoard.cookingDatabase.GetCellData(cellID).cellTexture);
        //cellMaterial.SetFloat("_Position", cellTexturePosition);
        //Vector4 tempSize = new Vector4(textureGridSize.x, textureGridSize.y, 0, 0);
        //cellMaterial.SetVector("_Grid", tempSize);
        //cellMaterial.color = BlackBoard.cookingDatabase.GetCellData(cellID).color;
    }

    public void UpdateVisual()
    {
        if (onGrid) cellVisual.transform.localPosition = new Vector3(0, 0, -0.1f * cellScale);
        else cellVisual.transform.localPosition = new Vector3(0, 0, -0.15f * cellScale);

        // Neighbor Offsets
        Vector2Int left = new Vector2Int(-1, 0);
        Vector2Int right = new Vector2Int(1, 0);
        Vector2Int top = new Vector2Int(0, 1);
        Vector2Int bottom = new Vector2Int(0, -1);
        Vector2Int topLeft = new Vector2Int(-1, 1);
        Vector2Int topRight = new Vector2Int(1, 1);
        Vector2Int bottomLeft = new Vector2Int(-1, -1);
        Vector2Int bottomRight = new Vector2Int(1, -1);

        // Dictionary for fast lookup of connected neighbors
        Dictionary<Vector2Int, bool> neighborMap = new Dictionary<Vector2Int, bool>();
        foreach (FoodCell cell in groupCells)
        {
            neighborMap[cell.gridPosition - gridPosition] = true;
        }

        // Helper function to check if a neighbor is connected
        bool IsConnected(Vector2Int offset) => neighborMap.ContainsKey(offset);

        // Assign visual parts based on connectivity rules
        Vector3Int Top = new Vector3Int(
            GetTopLeftVisual(IsConnected(left), IsConnected(top), IsConnected(topLeft)), // Top-Left
            GetTopVisual(IsConnected(top)), // Top
            GetTopRightVisual(IsConnected(right), IsConnected(top), IsConnected(topRight)) // Top-Right
        );

        Vector3Int Middle = new Vector3Int(
            GetLeftVisual(IsConnected(left)), // Left
            4, // Center always 4
            GetRightVisual(IsConnected(right)) // Right
        );

        Vector3Int Bottom = new Vector3Int(
            GetBottomLeftVisual(IsConnected(left), IsConnected(bottom), IsConnected(bottomLeft)), // Bottom-Left
            GetBottomVisual(IsConnected(bottom)), // Bottom
            GetBottomRightVisual(IsConnected(right), IsConnected(bottom), IsConnected(bottomRight)) // Bottom-Right
        );

        // Update cell visuals
        SetCellVisualMaterial(Top, Middle, Bottom);
        vfx.SetBool("North", !IsConnected(top));
        vfx.SetBool("South", !IsConnected(bottom));
        vfx.SetBool("West", !IsConnected(left));
        vfx.SetBool("East", !IsConnected(right));
    }

    private void SetCellVisualMaterial(Vector3Int top, Vector3Int middle, Vector3Int bottom)
    {
        Material cellMaterial = new Material(cellVisual.GetComponent<MeshRenderer>().material);
        cellMaterial.SetTexture("_BaseMap", cellData.cellTexture);
        Vector4 maskValue = new Vector4(top.x, top.y, top.z, 0);
        cellMaterial.SetVector("_Top", maskValue);
        maskValue = new Vector4(middle.x, middle.y, middle.z, 0);
        cellMaterial.SetVector("_Middle", maskValue);
        maskValue = new Vector4(bottom.x, bottom.y, bottom.z, 0);
        cellMaterial.SetVector("_Bottom", maskValue);
        cellMaterial.color = cellData.color;
        cellVisual.GetComponent<MeshRenderer>().material = cellMaterial;
    }



    // Determine visual values for each part based on neighbors
    private int GetTopLeftVisual(bool left, bool top, bool topLeft)
    {
        if (left && top && topLeft) return 4;
        if (left && top) return 9;
        if (top) return 5;
        if (left) return 7;
        return 8;
    }

    private int GetTopVisual(bool top)
    {
        if (!top) return 7;
        return 4;
    }

    private int GetTopRightVisual(bool right, bool top, bool topRight)
    {
        if (right && top && topRight) return 4;
        if (right && top) return 10;
        if (top) return 3;
        if (right) return 7;
        return 6;
    }

    private int GetLeftVisual(bool left)
    {
        if (!left) return 5;
        return 4;
    }

    private int GetRightVisual(bool right)
    {
        if (!right) return 3;
        return 4;
    }

    private int GetBottomLeftVisual(bool left, bool bottom, bool bottomLeft)
    {
        if (left && bottom && bottomLeft) return 4;
        if (left && bottom) return 11;
        if (bottom) return 5;
        if (left) return 1;
        return 2;
    }

    private int GetBottomVisual(bool bottom)
    {
        if (!bottom) return 1;
        return 4;
    }

    private int GetBottomRightVisual(bool right, bool bottom, bool bottomRight)
    {
        if (right && bottom && bottomRight) return 4;
        if (right && bottom) return 12;
        if (bottom) return 3;
        if (right) return 1;
        return 0;
    }
}
