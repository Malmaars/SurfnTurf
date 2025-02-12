using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridSize;
    private Vector3 gridRotation;
    public Sprite gridCellSprite;
    private GameObject gridPlane;

    [Header("PlayerProperties")]
    public Vector2Int mousePositionOnGrid;

    private List<FoodCell> cell;

    private void Awake()
    {
        gridPlane = transform.GetChild(0).gameObject;
        gridRotation = gridPlane.transform.eulerAngles;
        GenerateGridPlane();
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        GameObject gridHolder = new GameObject("GridHolder");
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
            }
        }

        gridHolder.transform.Rotate(gridRotation);
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.tag == "Grid")
            {
                mousePositionOnGrid = ConvertMousePosToGridPos(hit.point, hit.transform);
            }
        }
    }

    private Vector2Int ConvertMousePosToGridPos(Vector3 point, Transform hitTransform)
    {
        Vector4 tempPos = math.mul(hitTransform.worldToLocalMatrix, new Vector4(point.x, point.y, point.z, 1));
        GridManager gridManager = hitTransform.parent.GetComponent<GridManager>();
        Vector2Int gridPos = new Vector2Int((int)tempPos.x, (int)tempPos.y);
        return gridPos;
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
}
