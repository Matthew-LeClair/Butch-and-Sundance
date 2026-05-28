using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

// Generates a dynamic lightning effect between this object and a target
// using a LineRenderer and Perlin noise displacement.
public class Lightning : MonoBehaviour
{
    public Transform target; // Target the lightning will connect to
    public float displacementAmount = 2.0f; // Strength of the lightning distortion
    public int segments = 15; // Number of lightning segments

    private LineRenderer lineRenderer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Ensure a target has been assigned
        if (target != null)
        {
            
        }
    }

    public void GenerateLightning(Vector3 start, Vector3 end)
    {
        // Store all lightning points
        List<Vector3> pointList = new List<Vector3>();

        // Add the starting point
        pointList.Add(start);

        // Generate intermediate lightning segments
        for (int i = 1; i < segments; i++)
        {
            // Calculate percentage between start and end
            float percentage = (float)i / (float)segments;
            // Find the base position along the line
            Vector3 basePos = Vector3.Lerp(start, end, percentage);
            // Generate animated Perlin noise offset
            float offset = Mathf.PerlinNoise(i * 0.5f, Time.time * 5.0f) * 2.0f - 1.0f;
            // Apply displacement to create jagged lightning effect
            Vector3 randomOffset = new Vector3(offset * displacementAmount, offset * displacementAmount, 0);
            // Add displaced point to the list
            pointList.Add(basePos + randomOffset);
        }
        // Add the final endpoint
        pointList.Add(end);

        // Update the LineRenderer with generated points
        lineRenderer.positionCount = pointList.Count;
        lineRenderer.SetPositions(pointList.ToArray());
    }
}
