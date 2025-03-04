using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public string gridName;
    public Transform gridPivot;
    public bool testGrid;

    public Vector2Int gridSize;
    public bool generateGridCollider;
    public Sprite gridCellSprite;
    public GameObject foodCell;

    public List<FoodCell> cells;

    private Transform gridHolder;
    public Transform[,] gridPositions;
    public int[,] gridOccupation;
    public Transform cellHolder;
    public bool extractWhole;
    public float cellScale;

    public void ActivateGrid(float _cellScale)
    {
        cellScale = _cellScale;
        GenerateGrid();
        if (generateGridCollider)
        {
            Mesh mesh = GenerateGridCollider();
            transform.GetComponent<MeshFilter>().mesh = mesh;
            transform.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
        if(!testGrid) 
            transform.localScale = Vector3.zero;
    }

    public void ShowGrid()
    {
        if (testGrid) return;
        transform.position = gridPivot.position;
        transform.rotation = gridPivot.rotation;
        transform.localScale = Vector3.one;
    }

    public void HideGrid()
    {
        if (testGrid) return;
        transform.localScale = Vector3.zero;
    }

    public void GenerateGrid()
    {
        cells = new List<FoodCell>();

        gridHolder = new GameObject("GridHolder").transform;
        cellHolder = new GameObject("CellHolder").transform;
        gridHolder.parent = transform;
        cellHolder.parent = transform;
        gridHolder.transform.localPosition = Vector3.zero;
        cellHolder.transform.localPosition = Vector3.zero;
        gridHolder.transform.localRotation = Quaternion.identity;
        cellHolder.transform.localRotation = Quaternion.identity;
        gridHolder.gameObject.layer = 9; //layer 9 is InWorldUI
        cellHolder.gameObject.layer = 9;


        gridPositions = new Transform[gridSize.x, gridSize.y];
        gridOccupation = new int[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Transform gridPosition = new GameObject("GridCell(" + x + "," + y + ")").transform;

                if (gridCellSprite != null)
                {
                    gridPosition.gameObject.AddComponent<SpriteRenderer>();
                    gridPosition.GetComponent<SpriteRenderer>().sprite = gridCellSprite;
                }

                gridPosition.parent = gridHolder;
                gridPosition.localPosition = new Vector3(x+0.5f, y+0.5f, 0) * cellScale;
                gridPosition.localRotation = Quaternion.identity;
                gridPosition.localScale = Vector3.one * cellScale;
                //layer 5 is supposed to be UI
                gridPosition.gameObject.layer = 9;
                gridPositions[x, y] = gridPosition;
                gridOccupation[x, y] = 0;
            }
        }

    }

    public Mesh GenerateGridCollider()
    {

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(gridSize.x, 0, 0) * cellScale,
            new Vector3(0, gridSize.y, 0) * cellScale,
            new Vector3(gridSize.x, gridSize.y, 0) * cellScale
        };
        mesh.vertices = vertices;



        int[] tris = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        return mesh;
    }

    public void LoadIntoGrid(GridData data)
    {
        gridName = data.gridName;
        foreach (FoodCell cell in cells)
        {
            Destroy(cell.gameObject);
        }
        cells.Clear();  // Clear old data
        gridOccupation = new int[gridSize.x, gridSize.y];

        foreach (var foodData in data.foodCells)
        {
            GenerateCellOnGrid(foodData.x, foodData.y, foodData.id);
            //assign group variables;
        }

        foreach (var foodData in data.foodCells)
        {
            Vector2Int pos = new Vector2Int(foodData.x, foodData.y);
            FoodCell currentCell = cells.Find(cell => cell.gridPosition == pos);
            currentCell.SetGroup(CookingHelperFunctions.PositionsToCells(foodData.group, cells));
        }
    }

    public void GenerateCellOnGrid(int x, int y, int id)
    {
        Vector2Int gridPos = new Vector2Int(x, y);
        Vector2 worldPos = gridPositions[gridPos.x, gridPos.y].localPosition;

        FoodCell newCell = Instantiate(foodCell).GetComponent<FoodCell>();
        newCell.GenerateFoodCell(gridPos, worldPos, cellHolder, true, id, cellScale);
        gridOccupation[gridPos.x, gridPos.y] = 1;
        cells.Add(newCell);
    }

    public void SetCells(List<FoodCell> _cells, Vector2Int _onGridPosition)
    {
        foreach (FoodCell cell in _cells)
        {
            Vector2Int gridPos = cell.gridPosition + _onGridPosition;
            Vector2 worldPos = gridPositions[gridPos.x,gridPos.y].localPosition;
            cell.SetParent(cellHolder, true);
            cell.SetPosition(gridPos, worldPos);
            gridOccupation[gridPos.x, gridPos.y] = 1;
            cell.UpdateVisual();
            cells.Add(cell);
        }
    }
    public void RemoveCells()
    {
        cells.Clear();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                gridOccupation[x, y] = 0;
            }
        }
    }
    public void RemoveCells(List<FoodCell> _cells)
    {
        cells.RemoveAll(cell => _cells.Contains(cell));
        foreach (FoodCell cell in _cells)
        {
            gridOccupation[cell.gridPosition.x, cell.gridPosition.y] = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Mesh mesh = GenerateGridCollider();
        Gizmos.DrawMesh(mesh, -1, transform.position, transform.rotation, Vector3.one);
    }
}
