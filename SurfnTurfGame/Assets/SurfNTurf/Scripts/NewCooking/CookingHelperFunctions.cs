using UnityEngine;
using Unity.Mathematics;
using System;
using System.Collections.Generic;

public static class CookingHelperFunctions
{
    public static Vector2Int ConvertPointToGrid(Vector3 point, Transform hitTransform)
    {
        Vector4 tempPos = math.mul(hitTransform.worldToLocalMatrix, new Vector4(point.x, point.y, point.z, 1));
        Vector2Int gridPos = new Vector2Int((int)tempPos.x, (int)tempPos.y);
        return gridPos;
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

    public static List<Vector2Int> MapToPoints(int[,] map)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == 1)
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
        Vector2Int rotated;

        if (clockwise) rotated = new Vector2Int(translated.y, -translated.x);
        else rotated = new Vector2Int(-translated.y, translated.x);

        return rotated + pivot;
    }
}
