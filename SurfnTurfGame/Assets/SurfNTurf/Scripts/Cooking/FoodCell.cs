using UnityEngine;
using System.Collections.Generic;
using System;

public class FoodCell
{
    public Vector2Int gridPosition;
    public int cellRotation;

    public List<TagEnums.FlavourTag> flavourTags;
    public List<TagEnums.TextureTag> textureTags;
    public List<TagEnums.ColorTag> colorTags;

    //visual settings
    public GameObject foodCellVisual;

    public FoodCell(Vector2Int gridPosition, int cellRotation, 
                List<TagEnums.FlavourTag> flavourTags, List<TagEnums.TextureTag> textureTags, List<TagEnums.ColorTag> colorTags)
    {
        this.gridPosition = gridPosition;
        this.cellRotation = cellRotation;
        this.flavourTags = flavourTags;
        this.textureTags = textureTags;
        this.colorTags = colorTags;
    }

    public void GenerateVisual(GameObject cellVisual, Transform pieceHolder)
    {
        foodCellVisual = new GameObject("FoodCell of type: " + "temp");
        foodCellVisual.transform.parent = pieceHolder;
        foodCellVisual.AddComponent<FoodCellVisual>().GenerateVisual(cellVisual);
    }

    internal void SetStructure()
    {
        foodCellVisual.transform.localPosition = new Vector3(gridPosition.x, gridPosition.y, -0.1f);
    }
}