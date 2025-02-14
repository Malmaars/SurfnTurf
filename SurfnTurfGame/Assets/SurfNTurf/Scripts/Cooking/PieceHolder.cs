using System;
using UnityEngine;

public class PieceHolder : MonoBehaviour
{
    public void MoveObjectToGrid(Vector2Int mousePositionOnGrid, GridManager currentGridManager, float smoothing)
    {
        Vector3 mousePositionOnWorldGrid;
        Quaternion gridRotation;
        mousePositionOnWorldGrid = currentGridManager.gridCellVisuals[mousePositionOnGrid.x, mousePositionOnGrid.y].transform.position;
        gridRotation = currentGridManager.gridCellVisuals[mousePositionOnGrid.x, mousePositionOnGrid.y].transform.rotation;

        transform.position = Vector3.Lerp(transform.position, mousePositionOnWorldGrid, Time.deltaTime/smoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, gridRotation, Time.deltaTime / smoothing);
    }

    public void MoveObjectToMouse(Vector3 handPosition, float smoothing)
    {
        transform.position = Vector3.Lerp(transform.position, handPosition, Time.deltaTime / smoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime / smoothing);
    }
}