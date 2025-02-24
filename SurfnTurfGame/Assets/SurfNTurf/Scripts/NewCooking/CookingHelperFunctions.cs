using UnityEngine;
using Unity.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

public static class CookingHelperFunctions
{
    public static Vector2Int ConvertPointToGrid(Vector3 point, Transform hitTransform)
    {
        Vector4 tempPos = math.mul(hitTransform.worldToLocalMatrix, new Vector4(point.x, point.y, point.z, 1));
        Vector2Int gridPos = new Vector2Int((int)tempPos.x, (int)tempPos.y);
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

    public static Vector2Int RotatePosition(Vector2Int point, Vector2Int pivot, bool clockwise)
    {
        Vector2Int translated = point - pivot;
        Vector2Int rotatedPoint;

        if (clockwise) rotatedPoint = new Vector2Int(translated.y, -translated.x);
        else rotatedPoint = new Vector2Int(-translated.y, translated.x);

        return rotatedPoint + pivot;
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

    public static List<Vector2Int> CellsToPositions(List<FoodCell> cells)
    {
        List<Vector2Int> positions = cells.Select(cell => cell.gridPosition).ToList();
        return positions;
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
}