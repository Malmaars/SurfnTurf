using UnityEngine;
public class PieceHolder : MonoBehaviour
{
    [Range(0.01f, 1f)] 
    public float onGridSmoothing;
    [Range(0.01f, 1f)]
    public float aboveGridSmoothing;
    [Range(0.01f, 1f)]
    public float offGridSmoothing;

    public void MoveObjectToGrid(Vector2Int point, GridManager currentGridManager, Vector2 offset, float scale)
    {
        Vector3 mousePositionOnWorldGrid = currentGridManager.gridPositions[point.x, point.y].position;
        Quaternion gridRotation = currentGridManager.gridPositions[point.x, point.y].rotation;

        Vector3 finalPositionOnWorldGrid = mousePositionOnWorldGrid + (transform.right * (offset.x * scale)) + (transform.up * (offset.y * scale));

        transform.position = Vector3.Lerp(transform.position, finalPositionOnWorldGrid, Time.deltaTime / onGridSmoothing);
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