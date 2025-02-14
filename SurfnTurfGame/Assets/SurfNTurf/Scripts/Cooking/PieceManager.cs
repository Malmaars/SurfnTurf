using UnityEngine;
using System.Collections.Generic;
using System;

public class PieceManager : MonoBehaviour
{
    [SerializeField]
    private int[,] pieceShape = { { 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 0 }, { 0, 1, 1, 1, 0 }, { 0, 1, 1, 1, 0 }, { 0, 0, 0, 0, 0 } };
    public List<FoodCell> cells;
    private bool offsetSet;
    private Vector2Int celPosOffset;

    [Header("TempCellSettings")]
    public GameObject cellVisual;
    private Vector2Int cellPos;
    private List<TagEnums.FlavourTag> flavourTags;
    private List<TagEnums.TextureTag> textureTags;
    private List<TagEnums.ColorTag> colorTags;

    private void Start()
    {
        cells = new List<FoodCell>();
        SetPieceInfo();
        GeneratePiece();
    }

    private void SetPieceInfo()
    {
        for (int x = 0; x < pieceShape.GetLength(0); x++)
        {
            for (int y = 0; y < pieceShape.GetLength(1); y++)
            {
                if (pieceShape[x, y] == 1)
                {
                    cellPos = new Vector2Int(x, y);
                    if(!offsetSet)
                    {
                        celPosOffset = cellPos;
                        offsetSet = true;
                    }

                    cellPos -= celPosOffset;

                    FoodCell newCell = new FoodCell(cellPos, 0, flavourTags, textureTags, colorTags);
                    cells.Add(newCell);
                }
            }
        }
    }

    public void GeneratePiece()
    {
        foreach (FoodCell cell in cells)
        {
            cell.GenerateVisual(cellVisual, transform);
            cell.SetStructure();
        }
    }

    internal void ExtractWhole(List<FoodCell> foodCells)
    {
        throw new NotImplementedException();
    }

    internal void ExtractPiece(List<FoodCell> foodCells)
    {
        throw new NotImplementedException();
    }
}