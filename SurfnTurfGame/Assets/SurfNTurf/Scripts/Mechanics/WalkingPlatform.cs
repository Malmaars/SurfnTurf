using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

public class WalkingPlatform : MonoBehaviour
{
    public SplineContainer splineContainer; // Assign your SplineContainer in the inspector
    public float speed = 2f; // Units per second
    public List<Transform> platformTransforms; // Assign multiple platform transforms in the inspector

    private List<float> platformSplinePositions = new List<float>(); // Each platform's t value
    private List<Vector3> lastPositions = new List<Vector3>(); // Track previous position for each platform
    public float avoidRange = 2f; // Minimum distance between platforms

    void Start()
    {
        platformSplinePositions.Clear();
        lastPositions.Clear();

        for (int i = 0; i < platformTransforms.Count; i++)
        {
            float t = 0f;
            bool valid = false;
            int attempts = 0;
            const int maxAttempts = 100;

            while (!valid && attempts < maxAttempts)
            {
                t = Random.Range(0f, 1f);
                Vector3 candidatePos = splineContainer.EvaluatePosition(t);
                valid = true;

                // Check against all previously placed platforms
                for (int j = 0; j < i; j++)
                {
                    Vector3 otherPos = splineContainer.EvaluatePosition(platformSplinePositions[j]);
                    if (Vector3.Distance(candidatePos, otherPos) < avoidRange)
                    {
                        valid = false;
                        break;
                    }
                }
                attempts++;
            }

            platformSplinePositions.Add(t);
            lastPositions.Add(splineContainer.EvaluatePosition(t));
            if (platformTransforms[i] != null)
                platformTransforms[i].position = splineContainer.EvaluatePosition(t);
        }
    }

    // Returns the correct delta for the platform in the list
    public Vector3 GetPlatformDelta(Transform platformTransform)
    {
        int idx = platformTransforms.IndexOf(platformTransform);
        if (idx < 0 || idx >= lastPositions.Count) return Vector3.zero;
        return platformTransform.position - lastPositions[idx];
    }

    void Update()
    {
        if (splineContainer == null || splineContainer.Spline == null || platformTransforms == null || platformTransforms.Count == 0)
            return;

        for (int i = 0; i < platformTransforms.Count; i++)
        {
            var platformTransform = platformTransforms[i];
            if (platformTransform != null)
            {
                platformSplinePositions[i] += (speed / splineContainer.Spline.GetLength()) * Time.deltaTime;
                if (platformSplinePositions[i] > 1f)
                    platformSplinePositions[i] -= 1f;

                Vector3 position = splineContainer.EvaluatePosition(platformSplinePositions[i]);
                Vector3 tangent = splineContainer.EvaluateTangent(platformSplinePositions[i]);

                // Store previous position before updating
                lastPositions[i] = platformTransform.position;

                platformTransform.position = position;
                if (tangent != Vector3.zero)
                    platformTransform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
            }
        }
    }
}