#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(PushableObject), true)]
public class PushableObjectDrawer : Editor
{
    private float handleSize = 0.5f;

    void OnSceneGUI()
    {
        PushableObject pushable = (PushableObject)target;

        if (pushable.gridPositions == null)
            return;

        float handleSize = pushable.gridSize * 0.5f; // Use gridSize for handle size

        List<Vector2Int> emptyCells = new List<Vector2Int>();
        foreach (Vector2Int cell in pushable.gridPositions)
        {
            Vector2Int[] neighbors = new Vector2Int[]
            {
                cell + Vector2Int.up,
                cell + Vector2Int.down,
                cell + Vector2Int.left,
                cell + Vector2Int.right
            };
            foreach (var neighbor in neighbors)
            {
                if (!emptyCells.Contains(neighbor) && !pushable.gridPositions.Contains(neighbor))
                    emptyCells.Add(neighbor);
            }
        }

        foreach (Vector2Int cell in emptyCells)
        {
            Vector3 pos = pushable.GridToWorld(cell);
            Handles.color = Color.green;
            Handles.SphereHandleCap(0, pos, Quaternion.identity, handleSize, EventType.Repaint);

            if (Handles.Button(pos, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
            {
                pushable.gridPositions.Add(cell);
                EditorUtility.SetDirty(pushable);
            }
        }

        Vector2Int cellToRemove = Vector2Int.zero;
        bool remove = false;
        foreach (Vector2Int cell in pushable.gridPositions)
        {
            Vector3 pos = pushable.GridToWorld(cell);
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, pos, Quaternion.identity, handleSize, EventType.Repaint);

            if (Handles.Button(pos, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
            {
                cellToRemove = cell;
                remove = true;
            }
        }
        if (remove)
        {
            pushable.gridPositions.Remove(cellToRemove);
            EditorUtility.SetDirty(pushable);
        }
    }
}
#endif