using UnityEngine;
using System.Collections.Generic;

public class FoodCell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public int cellRotation;
    public GameObject cellVisual;
    public bool onGrid;
    public int cellID;

    public int cellTexturePosition;
    public Vector2Int textureGridSize;

    public List<FoodCell> groupCells;
    public List<FoodCell> neighborCells;

    public void GenerateFoodCell(Vector2Int _gridPosition, Vector2 _worldPosition, Transform _parent, bool _onGrid, int cellID, float cellScale, int _cellTexturePosition, Vector2Int _textureGridSize)
    {
        groupCells = new List<FoodCell>();
        neighborCells = new List<FoodCell>();

        SetParent(_parent, _onGrid);
        SetPosition(_gridPosition, _worldPosition);
        SetCellID(cellID);
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

    public void SetCellID(int _cellID)
    {
        cellID = _cellID;
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
            if(cell.gridPosition != gridPosition) groupCells.Add(cell);
        }
    }

    public void SetNeighbors(List<FoodCell> _grid)
    {
        Vector2Int[] neighborOffsets = {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1)
        };

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

    public void GenerateVisual(float _cellScale, int _cellTexturePosition, Vector2Int _textureGridSize)
    {
        cellVisual = Instantiate(cellVisual, transform);
        cellVisual.transform.localPosition += new Vector3(0, 0, -0.01f);
        cellVisual.transform.localScale = Vector3.one * _cellScale;
        //cellVisual.GetComponent<SpriteRenderer>().color = BlackBoard.cookingDatabase.GetCellData(cellID).color;

        cellTexturePosition = _cellTexturePosition;
        textureGridSize = _textureGridSize;

        Material cellMaterial = new Material(cellVisual.GetComponent<MeshRenderer>().material);
        cellMaterial.SetTexture("_BaseMap", BlackBoard.cookingDatabase.GetCellData(cellID).cellTexture);
        cellMaterial.SetFloat("_Position", cellTexturePosition);
        Vector4 tempSize = new Vector4(textureGridSize.x, textureGridSize.y, 0, 0);
        cellMaterial.SetVector("_Grid", tempSize);

        cellVisual.GetComponent<MeshRenderer>().material = cellMaterial;

        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if(onGrid) cellVisual.transform.localPosition = new Vector3(0, 0, -0.01f);
        else cellVisual.transform.localPosition = new Vector3(0, 0, -0.015f);
    }
}
