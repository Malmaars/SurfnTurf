using UnityEngine;
using System.Collections.Generic;
public class FoodCell
{
    public Vector2Int gridPosition;
    public int cellRotation;

    public List<TagEnums.FlavourTag> flavourTags;
    public List<TagEnums.TextureTag> textureTags;
    public List<TagEnums.ColorTag> colorTags;

    public FoodCell(Vector2Int gridPosition, int cellRotation, 
                List<TagEnums.FlavourTag> flavourTags, List<TagEnums.TextureTag> textureTags, List<TagEnums.ColorTag> colorTags)
    {
        this.gridPosition = gridPosition;
        this.cellRotation = cellRotation;
        this.flavourTags = flavourTags;
        this.textureTags = textureTags;
        this.colorTags = colorTags;
    }
}