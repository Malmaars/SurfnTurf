using UnityEngine;
using Unity.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

public static class CookingHelperFunctions
{
    public static Vector2Int ConvertPointToGrid(Vector3 point, Transform hitTransform, Vector2 offset, float scale)
    {
        Vector4 tempPos = math.mul(hitTransform.worldToLocalMatrix, new Vector4(point.x, point.y, point.z, 1));
        Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt((tempPos.x - offset.x * scale) / scale), Mathf.FloorToInt((tempPos.y - offset.y * scale) / scale));
        return gridPos;
    }

    public static Vector2Int GetMapCenter(List<FoodCell> cells)
    {
        Vector2Int mapCenter = Vector2Int.zero;
        List<Vector2Int> points = CellsToPositions(cells);

        Vector2Int bottomLeft = GetBottomLeftPoint(points);
        Vector2Int topRight = GetTopRightPoint(points);

        mapCenter.x = bottomLeft.x + (int)((topRight.x - bottomLeft.x) / 2);
        mapCenter.y = bottomLeft.y + (int)((topRight.y - bottomLeft.y) / 2);

        return mapCenter;
    }

    public static Vector2Int GetMapCenter(int[,] map)
    {
        Vector2Int mapCenter = Vector2Int.zero;
        List<Vector2Int> points = MapToPoints(map);

        Vector2Int bottomLeft = GetBottomLeftPoint(points);
        Vector2Int topRight = GetTopRightPoint(points);

        mapCenter.x = bottomLeft.x + (int)((topRight.x - bottomLeft.x) / 2);
        mapCenter.y = bottomLeft.y + (int)((topRight.y - bottomLeft.y) / 2);

        return mapCenter;
    }

    public static Vector2 GetPreciseCenter(List<FoodCell> cells)
    {
        Vector2 mapCenter = Vector2.zero;
        List<Vector2Int> points = CellsToPositions(cells);

        Vector2Int bottomLeft = GetBottomLeftPoint(points);
        Vector2Int topRight = GetTopRightPoint(points);

        if((int)(topRight.x - bottomLeft.x) % 2 == 1)
        {
            mapCenter.x = 0.5f;
        }
        if ((int)(topRight.y - bottomLeft.y) % 2 == 1)
        {
            mapCenter.y = 0.5f;
        }

        return mapCenter;
    }

    public static Vector3 GetWorldCenterFromPoints(List<FoodCell> cells)
    {

        Vector3 bottomLeft = GetBottomLeftPoint(cells);
        Vector3 topRight = GetTopRightPoint(cells);

        Vector3 mapCenter = bottomLeft + ((topRight - bottomLeft) / 2);

        return mapCenter;
    }

    public static List<int> MapToIDs(int[,] map)
    {
        List<int> IDs = new List<int>();

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] != 0)
                {
                    IDs.Add(map[x, y]);
                }
            }
        }

        return IDs;
    }

    public static int[,] GetMapFrom1DArray(IngredientData _data) { return GetMapFrom1DArray(_data.shape, _data.rows, _data.columns); }
    public static int[,] GetMapFrom1DArray(int[] shape, int rows, int columns)
    {
        int[,] map = new int[rows, columns];

        // Loop through the 1D array and map its values into the 2D array
        for (int i = 0; i < shape.Length; i++)
        {
            int row = i / columns;  // Calculate the row index
            int col = i % columns;  // Calculate the column index

            // Assign the value from the 1D array to the 2D array
            map[row, col] = shape[i];
        }

        return map;
    }

    public static int[,] GetMapFrom1DArray(List<FoodCellData> data, int rows, int columns)
    {
        int[,] map = new int[rows, columns];

        foreach (FoodCellData cell in data)
        {
            map[cell.x, cell.y] = 1;
        }

        return map;
    }

    public static List<Vector2Int> MapToPoints(int[,] map)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] != 0)
                {
                    points.Add(new Vector2Int(x, y));
                }
            }
        }

        return points;
    }

    public static List<int> MapToTexturePositions(int[,] map)
    {
        List<int> texturePositions = new List<int>();
        int texturePosition = 0;

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texturePosition = width * y + x;
                if (map[x, y] != 0)
                {
                    texturePositions.Add(texturePosition);
                }
                
            }
        }

        return texturePositions;
    }

    public static FoodCell PieceCompatible(List<FoodCell> cells, Vector2Int onGridPosition, GridManager gridManager)
    {

        FoodCell selectedCell = null;
        //get all positions
        List<Vector2Int> collidingPositions = new List<Vector2Int>(); 
        foreach (FoodCell cell in cells)
        {
            if (cell.gridPosition.x + onGridPosition.x < 0 ||
                cell.gridPosition.x + onGridPosition.x > gridManager.gridSize.x - 1 ||
                cell.gridPosition.y + onGridPosition.y < 0 ||
                cell.gridPosition.y + onGridPosition.y > gridManager.gridSize.y - 1)
                return selectedCell;
            if (gridManager.gridShape[cell.gridPosition.x + onGridPosition.x, cell.gridPosition.y + onGridPosition.y] == 0)
                return selectedCell;
            if (!gridManager.allowAlteredCells && cell.altered)
                return selectedCell;
            if (gridManager.gridOccupation[cell.gridPosition.x + onGridPosition.x, cell.gridPosition.y + onGridPosition.y] == 1)
                collidingPositions.Add(new Vector2Int(cell.gridPosition.x + onGridPosition.x, cell.gridPosition.y + onGridPosition.y));
        }

        if (collidingPositions.Count == 0)
            return selectedCell;

        int foundPiecesCount = 0;
        foreach (Vector2Int gridPos in collidingPositions)
        {
            //find the cell in gridManager.cells with FoodCell.gridPosition that is the same as gridPos
            // Find the corresponding FoodCell in gridManager.cells
            foreach (FoodCell foodCell in gridManager.cells)
            {
                if (foodCell.gridPosition == gridPos && foodCell.found == false)
                {
                    foundPiecesCount++;
                    if(foundPiecesCount > 1)
                    {
                        foreach (FoodCell cell in gridManager.cells)
                        {
                            cell.found = false;
                        }
                        if (gridManager.extractWhole)
                        {
                            return selectedCell;
                        }
                        else
                        {
                            selectedCell = null;
                            return selectedCell;
                        }
                    }
                    selectedCell = foodCell; // Assign the first found food cell (or modify logic if needed)
                    selectedCell.found = true;

                    Queue<FoodCell> toCheck = new Queue<FoodCell>();

                    foreach (FoodCell cell in selectedCell.groupCells)
                    {
                        cell.found = true;
                        toCheck.Enqueue(cell);
                    }

                    while (toCheck.Count > 0)
                    {
                        FoodCell current = toCheck.Dequeue();
                        // Check its connected cells
                        foreach (FoodCell neighbor in current.groupCells)
                        {
                            if (!neighbor.found)
                            {
                                neighbor.found = true;
                                toCheck.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }
        }

        foreach (FoodCell cell in gridManager.cells)
        {
            cell.found = false;
        }

        return selectedCell;
    }

    public static Vector2Int GetBottomLeftPoint(List<Vector2Int> points)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;

        foreach (Vector2Int point in points)
        {
            if (point.x < minX) minX = point.x;
            if (point.y < minY) minY = point.y;
        }

        return new Vector2Int(minX, minY);
    }

    public static Vector3 GetBottomLeftPoint(List<FoodCell> cells)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;

        Vector3 pos = Vector3.zero;

        foreach (FoodCell cell in cells)
        {
            pos = cell.transform.position;
            if (pos.x < minX) minX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.z < minZ) minZ = pos.z;
        }

        return new Vector3(minX, minY, minZ);
    }

    public static Vector2Int GetTopRightPoint(List<Vector2Int> points)
    {
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (Vector2Int point in points)
        {
            if (point.x > maxX) maxX = point.x;
            if (point.y > maxY) maxY = point.y;
        }

        return new Vector2Int(maxX, maxY);
    }

    public static Vector3 GetTopRightPoint(List<FoodCell> cells)
    {
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;

        Vector3 pos = Vector3.zero;

        foreach (FoodCell cell in cells)
        {
            pos = cell.transform.position;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
            if (pos.z > maxZ) maxZ = pos.z;
        }

        return new Vector3(maxX, maxY, maxZ);
    }

    public static Vector2Int RotatePosition(Vector2Int point, Vector2 pivot, bool clockwise)
    {
        // Translate point so pivot becomes the origin
        Vector2 translated = point - pivot;
        Vector2 rotatedPoint;

        // Perform 90-degree rotation
        if (clockwise)
            rotatedPoint = new Vector2(translated.y, -translated.x);
        else
            rotatedPoint = new Vector2(-translated.y, translated.x);

        // Rotate the pivot around the origin
        Vector2 rotatedPivot = new Vector2(pivot.y, pivot.x);

        // Translate back using the rotated pivot
        Vector2 finalPoint = rotatedPoint + rotatedPivot;

        return new Vector2Int(Mathf.RoundToInt(finalPoint.x), Mathf.RoundToInt(finalPoint.y));
    }

    public static Vector2Int RotatePoint(Vector2Int point, Vector2 pivot, bool clockwise)
    {
        Vector2 translated = point - pivot;
        Vector2 rotatedPoint;

        if (clockwise)
            rotatedPoint = new Vector2(translated.y, -translated.x);
        else
            rotatedPoint = new Vector2(-translated.y, translated.x);

        Vector2 finalPoint = rotatedPoint + pivot;

        return new Vector2Int(Mathf.RoundToInt(finalPoint.x), Mathf.RoundToInt(finalPoint.y));
    }

    public static List<Vector2Int> RotatePoints(List<Vector2Int> points, Vector2Int pivot, bool clockwise)
    {
        List<Vector2Int> newPoints = new List<Vector2Int>();

        foreach (Vector2Int point in points)
        {
            Vector2Int newPoint = RotatePosition(point, pivot, clockwise);
            newPoints.Add(newPoint);
        }

        return newPoints;
    }

    public static List<Vector2Int> RotatePoints(List<FoodCell> cells, Vector2Int pivot, Vector2Int size, bool clockwise)
    {
        if (size.x != size.y)
        {
            Debug.LogError("Size must be square!");
            return null;
        }

        int n = size.x;

        List<Vector2Int> points = CellsToPositions(cells);
        List<Vector2Int> rotatedPositions = new List<Vector2Int>();

        foreach (Vector2Int point in points)
        {
            int localX = point.x - pivot.x;
            int localY = point.y - pivot.y;

            if (localX < 0 || localX >= n || localY < 0 || localY >= n)
            {
                Debug.LogWarning($"Cell at {point} is outside the defined area and will be ignored.");
                continue;
            }

            int rotatedLocalX, rotatedLocalY;

            if (clockwise)
            {
                rotatedLocalX = localY;
                rotatedLocalY = n - 1 - localX;
            }
            else
            {
                rotatedLocalX = n - 1 - localY;
                rotatedLocalY = localX;
            }

            Vector2Int rotatedPos = new Vector2Int(pivot.x + rotatedLocalX, pivot.y + rotatedLocalY);
            rotatedPositions.Add(rotatedPos);
        }

        return rotatedPositions;
    }

    public static List<Vector2Int> CellsToPositions(List<FoodCell> cells)
    {
        List<Vector2Int> positions = cells.Select(cell => cell.gridPosition).ToList();
        return positions;
    }

    public static List<FoodCell> PositionsToCells(List<Vector2Int> positions, List<FoodCell> cells)
    {
        List<FoodCell> matchedCells = new List<FoodCell>();

        foreach (Vector2Int pos in positions)
        {
            FoodCell foundCell = cells.Find(cell => cell.gridPosition == pos);
            if (foundCell != null)
            {
                matchedCells.Add(foundCell);
            }
        }

        return matchedCells;
    }

    public static Vector2Int GetFirstCellPosition(int[,] map)
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] != 0)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return Vector2Int.zero;
    }

    public static bool GridCompatible(List<FoodCell> cells, Vector2Int onGridPosition, GridManager gridManager)
    {
        foreach (FoodCell cell in cells)
        {
            if (cell.gridPosition.x + onGridPosition.x < 0 ||
                cell.gridPosition.x + onGridPosition.x > gridManager.gridSize.x - 1 ||
                cell.gridPosition.y + onGridPosition.y < 0 ||
                cell.gridPosition.y + onGridPosition.y > gridManager.gridSize.y - 1)
                return false;
            if (gridManager.gridShape[cell.gridPosition.x + onGridPosition.x, cell.gridPosition.y + onGridPosition.y] == 0)
                return false;
            if (!gridManager.allowAlteredCells && cell.altered)
                return false;
            if (gridManager.gridOccupation[cell.gridPosition.x + onGridPosition.x, cell.gridPosition.y + onGridPosition.y] == 1)
                return false;
        }
        return true;
    }

    public static bool GridCompatible(List<Vector2Int> points, Vector2Int center, int[,] gridOccupation, Vector2Int onGridPosition)
    {
        foreach (Vector2Int point in points)
        {
            Vector2Int pos = onGridPosition + point - center;
            if (pos.x < 0 ||
            pos.x > gridOccupation.GetLength(0) - 1 ||
            pos.y < 0 ||
            pos.y > gridOccupation.GetLength(1) - 1)
                return false;
            if (gridOccupation[pos.x, pos.y] != 0)
                return false;
        }

        return true;
    }

    public static bool GridCompatible(Vector2Int[] points, int[,] gridShape, Vector2Int onGridPosition)
    {
        foreach (Vector2Int point in points)
        {
            Vector2Int pos = onGridPosition + point;
            if (pos.x < 0 ||
            pos.x > gridShape.GetLength(0) - 1 ||
            pos.y < 0 ||
            pos.y > gridShape.GetLength(1) - 1)
                return false;
            if (gridShape[pos.x, pos.y] == 0)
                return false;
        }

        return true;
    }

    public static List<FoodCell> ExtractConnectedCells(FoodCell selectedCell)
    {
        List<FoodCell> connectedCells = new List<FoodCell>();
        Queue<FoodCell> toCheck = new Queue<FoodCell>();

        // Start with the selected cell
        selectedCell.found = true;
        foreach (FoodCell cell in selectedCell.groupCells)
        {
            cell.found = true;
            toCheck.Enqueue(cell);
        }

        while (toCheck.Count > 0)
        {
            FoodCell current = toCheck.Dequeue();
            connectedCells.Add(current);

            // Check its connected cells
            foreach (FoodCell neighbor in current.groupCells)
            {
                if (!neighbor.found)
                {
                    neighbor.found = true;
                    toCheck.Enqueue(neighbor);
                }
            }
        }

        // Reset 'found' so it can be used again
        foreach (FoodCell cell in connectedCells)
        {
            cell.found = false;
        }
        selectedCell.found = false;

        return connectedCells;
    }
}

