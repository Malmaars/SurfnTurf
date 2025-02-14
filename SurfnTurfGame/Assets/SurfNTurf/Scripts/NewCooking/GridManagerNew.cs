using UnityEngine;
using System.Collections.Generic;

public class GridManagerNew : MonoBehaviour
{
    public Vector2Int gridSize;
    public bool generateGridCollider;
    public Sprite gridCellSprite;

    public List<FoodCellNew> cells;

    private Transform gridHolder;
    private Transform[,] gridPositions;
    private Transform cellHolder;

    private void Start()
    {
        GenerateGrid();
        if (generateGridCollider) GenerateGridCollider();

    }

    public void GenerateGrid()
    {
        cells = new List<FoodCellNew>();

        gridHolder = new GameObject("GridHolder").transform;
        cellHolder = new GameObject("CellHolder").transform;
        gridHolder.parent = transform;
        cellHolder.parent = transform;
        gridHolder.transform.localPosition = Vector3.zero;
        cellHolder.transform.localPosition = Vector3.zero;
        gridHolder.transform.localRotation = Quaternion.identity;
        cellHolder.transform.localRotation = Quaternion.identity;

        gridPositions = new Transform[gridSize.x, gridSize.y];

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
                gridPosition.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                gridPosition.localRotation = Quaternion.identity;
                //gridPosition.localScale = Vector3.zero;
                gridPositions[x, y] = gridPosition;
            }
        }

    }

    public void GenerateGridCollider()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(gridSize.x, 0, 0),
            new Vector3(0, gridSize.y, 0),
            new Vector3(gridSize.x, gridSize.y, 0)
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

        transform.GetComponent<MeshFilter>().mesh = mesh;
        transform.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void SetCells(List<FoodCellNew> _cells, Vector2Int _offset, Vector2Int _onGridPosition)
    {
        foreach (FoodCellNew cell in _cells)
        {
            cell.SetParent(gridHolder);
            cell.SetPosition(cell.gridPosition - _offset + _onGridPosition);
        }
    }

    public void RemoveCells()
    {

    }
}
