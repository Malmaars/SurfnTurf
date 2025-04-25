using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Steamworks;

public class GridManager : MonoBehaviour
{
    public string gridName;
    public Transform gridPivot;
    public bool testGrid;
    public bool canBeSaved;
    private bool activated;

    public Vector2Int gridSize;
    public bool generateGridCollider;
    public GameObject gridCellVisual;
    public GameObject foodCell;

    public List<FoodCell> cells;

    private Transform gridHolder;
    public Transform[,] gridPositions;
    public List<GameObject> gridCellVisuals;
    public IngredientData gridShapeData;
    public int[,] gridShape;
    public int[,] gridOccupation;
    public Transform cellHolder;
    public bool extractWhole;
    public float cellScale;
    public bool customScale;
    public bool allowAlteredCells;
    public bool alwaysOn = true;
    public bool turnedOn;
    public bool forceSeparation;
    public bool alterCells;
    public bool showScore;
    public bool showGridCellVisuals = true;
    public bool mayExtract = true;

    public void ActivateGrid(float _cellScale)
    {
        if (activated)
            return;
        activated = true;
        if (!customScale)
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
        //if (testGrid) return;

        //transform.position = gridPivot.position;
        //transform.rotation = gridPivot.rotation;

        gameObject.SetActive(true);
    }

    public void HideGrid()
    {
        //if (testGrid) return;
        gameObject.SetActive(false);
    }

    public virtual void TurnOn()
    {
        turnedOn = true;
    }

    public virtual void TurnOff()
    {
        turnedOn = false;
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

        gridShape = new int[gridSize.x, gridSize.y];

        gridPositions = new Transform[gridSize.x, gridSize.y];
        gridOccupation = new int[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Transform gridPosition;

                if (gridShapeData != null)
                    gridShape[x, y] = gridShapeData.GetValue(x, y);
                else
                    gridShape[x, y] = 1;


                if (gridCellVisual == null)
                {
                    gridPosition = new GameObject("GridCell(" + x + "," + y + ")").transform;
                }
                else
                {
                    gridPosition = Instantiate(gridCellVisual).transform;
                }

                

                gridPosition.parent = gridHolder;
                gridPosition.localPosition = new Vector3(x+0.5f, y+0.5f, 0) * cellScale;
                gridPosition.localRotation = Quaternion.identity;
                gridPosition.localScale = Vector3.one * cellScale;
                //layer 9 is InWorldUI
                gridPosition.gameObject.layer = 9;
                gridCellVisuals.Add(gridPosition.gameObject);
                gridPositions[x, y] = gridPosition;
                if (gridShape[x, y] == 0 || !showGridCellVisuals)
                {
                    gridPosition.gameObject.SetActive(false);
                }
                gridOccupation[x, y] = 0;
            }
        }

        gridHolder.transform.localScale = Vector3.one;
        cellHolder.transform.localScale = Vector3.one;

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

        ClearGrid();

        foreach (var foodData in data.foodCells)
        {
            GenerateCellOnGrid(foodData.x, foodData.y, foodData.id, foodData.texturePosition, foodData.textureGridSize, foodData.originalIngredient);
            //assign group variables;
        }


        foreach (var foodData in data.foodCells)
        {
            Vector2Int pos = new Vector2Int(foodData.x, foodData.y);
            FoodCell currentCell = cells.Find(cell => cell.gridPosition == pos);
            currentCell.SetGroup(CookingHelperFunctions.PositionsToCells(foodData.group, cells));
            currentCell.UpdateVisual();
        }

        foreach (FoodCell cell in cells)
        {
            cell.SetNeighbors(cells);
        }
    }

    public void ClearGrid()
    {
        foreach (FoodCell cell in cells)
        {
            Destroy(cell.gameObject);
        }
        cells.Clear();  // Clear old data
        gridOccupation = new int[gridSize.x, gridSize.y];
    }

    public void GenerateCellOnGrid(int x, int y, int id, int texturePosition, Vector2Int textureGridSize, string originalIngredient)
    {
        Vector2Int gridPos = new Vector2Int(x, y);
        Vector2 worldPos = gridPositions[gridPos.x, gridPos.y].localPosition;

        FoodCell newCell = Instantiate(foodCell).GetComponent<FoodCell>();
        newCell.GenerateFoodCell(gridPos, worldPos, cellHolder, true, id, cellScale, texturePosition, textureGridSize, originalIngredient);
        gridOccupation[gridPos.x, gridPos.y] = 1;
        cells.Add(newCell);
    }

    public virtual void SetCells(List<FoodCell> _cells, Vector2Int _onGridPosition)
    {
        foreach (FoodCell cell in _cells)
        {
            Vector2Int gridPos = cell.gridPosition + _onGridPosition;
            Vector2 worldPos = gridPositions[gridPos.x,gridPos.y].localPosition;
            cell.SetParent(cellHolder, true);
            cell.SetPosition(gridPos, worldPos);
            if (forceSeparation)
                cell.SetGroup(null);
            gridOccupation[gridPos.x, gridPos.y] = 1;
            if(alterCells)
                cell.altered = true;
            cells.Add(cell);
        }
        foreach (FoodCell cell in _cells)
        {
            cell.UpdateVisual();
        }

        foreach (FoodCell cell in cells)
        {
            cell.SetNeighbors(cells);
        }
        

        if (IsGridFullyOccupied())
        {
            //cookingManager.LoadNextGrid();
            StartCoroutine(PlayFilledEffect(_cells));
            if (SteamManager.Initialized)
            {
                Steamworks.SteamUserStats.GetAchievement("SURF_N_TEST", out bool achievementCompleted);

                if (!achievementCompleted)
                {
                    SteamUserStats.SetAchievement("SURF_N_TEST");
                    SteamUserStats.StoreStats();
                }
            }
        }

        else
        {
            foreach (FoodCell cell in _cells)
            {
                cell.PlayEffect("OnRelease");
            }
        }
    }
    public virtual void RemoveCells()
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
    public virtual void RemoveCells(List<FoodCell> _cells)
    {
        cells.RemoveAll(cell => _cells.Contains(cell));
        foreach (FoodCell cell in _cells)
        {
            gridOccupation[cell.gridPosition.x, cell.gridPosition.y] = 0;
        }

        foreach (FoodCell cell in cells)
        {
            cell.SetNeighbors(cells);
        }
    }

    public bool IsGridFullyOccupied()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                if (gridShape[x, y] == 1 && gridOccupation[x, y] == 0)
                    return false;
            }
        }
        return true;
    }

    IEnumerator PlayFilledEffect(List<FoodCell> _cells)
    {
        Queue<List<FoodCell>> expansionQueue = new Queue<List<FoodCell>>();
        HashSet<FoodCell> visited = new HashSet<FoodCell>();

        Dictionary<Vector2Int, FoodCell> cellDictionary = new Dictionary<Vector2Int, FoodCell>();
        foreach (var cell in cells)
        {
            cellDictionary[cell.gridPosition] = cell;
        }

        expansionQueue.Enqueue(new List<FoodCell>(_cells));
        foreach (var cell in _cells)
        {
            visited.Add(cell);
        }

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (expansionQueue.Count > 0)
        {
            List<FoodCell> currentWave = expansionQueue.Dequeue();

            foreach (FoodCell cell in currentWave)
            {
                cell.PlayEffect("OnCompleted");
            }

            List<FoodCell> nextWave = new List<FoodCell>();
            foreach (FoodCell cell in currentWave)
            {
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighborPos = cell.gridPosition + dir;

                    if (cellDictionary.TryGetValue(neighborPos, out FoodCell neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        nextWave.Add(neighbor);
                    }
                }
            }

            if (nextWave.Count > 0)
            {
                expansionQueue.Enqueue(nextWave);
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Mesh mesh = GenerateGridCollider();
        Gizmos.DrawMesh(mesh, -1, transform.position, transform.rotation, Vector3.one);
    }
}
