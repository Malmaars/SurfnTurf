using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using System.Collections;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridSize;
    public bool lockPiece;
    private Vector3 gridRotation;
    public Sprite gridCellSprite;
    private GameObject gridPlane;
    private GameObject gridHolder;
    public float cellOffset;

    public List<FoodCell> cells;
    public int[,] gridOccupation;

    [HideInInspector]
    public GameObject[,] gridCellVisuals;

    private void Awake()
    {
        cells = new List<FoodCell>();
        gridPlane = transform.GetChild(0).gameObject;
        gridRotation = gridPlane.transform.eulerAngles;
        gridCellVisuals = new GameObject[gridSize.x,gridSize.y];
        gridOccupation = new int[gridSize.x, gridSize.y];
        GenerateGridPlane();
        GenerateGrid();
        StartCoroutine(ShowGrid());
    }

    public void PlacePiece(List<FoodCell> cells, Vector2Int onGridPosition)
    {
        foreach (FoodCell cell in cells)
        {
            cell.gridPosition = cell.gridPosition + onGridPosition;
            cell.foodCellVisual.transform.parent = gridHolder.transform;
            cell.foodCellVisual.transform.localPosition = gridCellVisuals[cell.gridPosition.x, cell.gridPosition.y].transform.localPosition;
            cell.foodCellVisual.transform.localRotation = Quaternion.identity;
        }
    }


    //Grid Generation
    private IEnumerator ShowGrid()
    {
        yield return new WaitForSeconds(1);
        foreach (GameObject gridCell in gridCellVisuals)
        {
            while(gridCell.transform.localScale.x < 1)
            {
                gridCell.transform.localScale += Vector3.one * 10 * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            gridCell.transform.localScale = Vector3.one;
        }
        yield return null;
    }

    public List<FoodCell> ExtractPiece(Vector2Int onGridPosition)
    {

        return cells;
    }

    public List<FoodCell> ExtractWhole()
    {
        return cells;
    }

    public void GenerateGrid()
    {
        gridHolder = new GameObject("GridHolder");
        gridHolder.transform.parent = this.transform;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                GameObject gridCell = new GameObject("GridCell("+ x + "," + y +")");
                gridCell.AddComponent<SpriteRenderer>();
                gridCell.GetComponent<SpriteRenderer>().sprite = gridCellSprite;
                gridCell.transform.parent = gridHolder.transform;
                gridCell.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                gridCell.transform.localScale = Vector3.zero;
                gridCellVisuals[x,y] = gridCell;
            }
        }

        gridHolder.transform.localPosition = Vector3.zero;
        gridHolder.transform.Rotate(gridRotation);
    }

    private void GenerateGridPlane()
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

        gridPlane.GetComponent<MeshFilter>().mesh = mesh;
        gridPlane.GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    //--------------
}
