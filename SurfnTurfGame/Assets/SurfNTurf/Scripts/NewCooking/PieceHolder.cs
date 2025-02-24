using UnityEngine;
public class PieceHolder : MonoBehaviour
{
    [Range(0.01f, 1f)] 
    public float onGridSmoothing;
    [Range(0.01f, 1f)]
    public float aboveGridSmoothing;
    [Range(0.01f, 1f)]
    public float offGridSmoothing;

    public void MoveObjectToGrid(Vector2Int point, GridManager currentGridManager)
    {
        Vector3 mousePositionOnWorldGrid;
        Quaternion gridRotation;
        mousePositionOnWorldGrid = currentGridManager.gridPositions[point.x, point.y].position;
        gridRotation = currentGridManager.gridPositions[point.x, point.y].rotation;

        transform.position = Vector3.Lerp(transform.position, mousePositionOnWorldGrid, Time.deltaTime / onGridSmoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, gridRotation, Time.deltaTime / onGridSmoothing);
    }

    public void MoveObjectAboveGrid(Vector3 point, Quaternion rotation)
    {
        transform.position = Vector3.Lerp(transform.position, point, Time.deltaTime / aboveGridSmoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime / aboveGridSmoothing);
    }

    public void MoveObjectToPoint(Vector3 point)
    {
        transform.position = Vector3.Lerp(transform.position, point, Time.deltaTime / offGridSmoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime / offGridSmoothing);
    }
}