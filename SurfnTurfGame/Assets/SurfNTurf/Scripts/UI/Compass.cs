using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    [SerializeField] RectTransform NorthIcon;
    [SerializeField] RectTransform SouthIcon;
    [SerializeField] RectTransform EastIcon;
    [SerializeField] RectTransform WestIcon;
    [SerializeField] float compassWidth = 600f; // Set this to your compass bar width in the inspector
    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] private RectTransform waypointParent;
    private List<WayPoint> wayPoints = new List<WayPoint>();

    private TMP_Text northText, southText, eastText, westText;

    private void Awake()
    {
        northText = NorthIcon.GetComponent<TMP_Text>();
        southText = SouthIcon.GetComponent<TMP_Text>();
        eastText = EastIcon.GetComponent<TMP_Text>();
        westText = WestIcon.GetComponent<TMP_Text>();
    }

    private void Start()
    {
        compassWidth = GetComponent<RectTransform>().sizeDelta.x;
    }

    private void Update()
    {
        UpdateCompass();
        UpdateWaypoints();
    }
    public WayPoint AddWaypoint(Vector3 position)
    {
        if (waypointPrefab == null)
        {
            Debug.LogError("Waypoint prefab is not assigned.");
            return new WayPoint();
        }
        GameObject waypointObject = Instantiate(waypointPrefab, waypointParent.position, waypointParent.rotation, waypointParent);
        WayPoint wayPoint = new WayPoint(position, waypointObject);
        wayPoints.Add(wayPoint );
        return wayPoint;
    }
    public void RemoveWaypoint(WayPoint wayPoint)
    {
        wayPoints.Remove(wayPoint);
        Destroy(wayPoint.waypointObject);
    }

    private void UpdateCompass()
    {
        float playerYaw = BlackBoard.cameraController.currentCamera.transform.eulerAngles.y;

        SetIcon(NorthIcon, northText, 0, playerYaw);
        SetIcon(EastIcon, eastText, 90, playerYaw);
        SetIcon(SouthIcon, southText, 180, playerYaw);
        SetIcon(WestIcon, westText, 270, playerYaw);
    }

    private void SetIcon(RectTransform icon, TMP_Text text = null, float worldAngle = 0, float playerYaw = 0, Image image = null)
    {
        float angleDiff = Mathf.DeltaAngle(playerYaw, worldAngle);
        float maxAngle = 90f;
        float halfWidth = compassWidth / 2f;
        float x = Mathf.Clamp(angleDiff / maxAngle, -1f, 1f) * halfWidth;

        icon.anchoredPosition = new Vector2(x, 0);

        float fade = Mathf.Clamp01(1f - Mathf.Abs(angleDiff) / maxAngle);
        if (text != null)
        {
            Color c = text.color;
            c.a = fade;
            text.color = c;
        }
        if (image != null)
        {
            Color c = image.color;
            c.a = fade;
            image.color = c;
        }
    }

    private void UpdateWaypoints()
    {
        float playerYaw = BlackBoard.cameraController.currentCamera.transform.eulerAngles.y;
        Vector3 playerPos = BlackBoard.playerBody.transform.position;

        foreach (var wp in wayPoints)
        {
            if (wp.waypointObject == null) continue;

            // Calculate angle to waypoint in world space
            Vector3 toWaypoint = wp.position - playerPos;
            float angleToWaypoint = Mathf.Atan2(toWaypoint.x, toWaypoint.z) * Mathf.Rad2Deg;

            // Use the same SetIcon logic as for N/E/S/W
            var rect = wp.waypointObject.GetComponent<RectTransform>();
            var image = wp.waypointObject.GetComponent<Image>();
            SetIcon(rect, null, angleToWaypoint, playerYaw, image);
        }
    }
}
public struct WayPoint
{
    public Vector3 position;
    public GameObject waypointObject;

    public WayPoint(Vector3 position, GameObject waypointObject = null)
    {
        this.position = position;
        this.waypointObject = waypointObject;
    }
}
