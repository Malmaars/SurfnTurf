using UnityEngine;
public class PieceHolder : MonoBehaviour
{
    [Range(0.01f, 1f)] 
    public float onGridSmoothing;
    [Range(0.01f, 1f)]
    public float offGridSmoothing;

    public void MoveObjectToGrid(Vector2Int mousePositionOnGrid, GridManager currentGridManager)
    {
        Vector3 mousePositionOnWorldGrid;
        Quaternion gridRotation;
        mousePositionOnWorldGrid = currentGridManager.gridPositions[mousePositionOnGrid.x, mousePositionOnGrid.y].position;
        gridRotation = currentGridManager.gridPositions[mousePositionOnGrid.x, mousePositionOnGrid.y].rotation;

        transform.position = Vector3.Lerp(transform.position, mousePositionOnWorldGrid, Time.deltaTime / onGridSmoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, gridRotation, Time.deltaTime / onGridSmoothing);
    }

    public void MoveObjectToPoint(Vector3 point)
    {
        transform.position = Vector3.Lerp(transform.position, point, Time.deltaTime / offGridSmoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime / offGridSmoothing);
    }
}