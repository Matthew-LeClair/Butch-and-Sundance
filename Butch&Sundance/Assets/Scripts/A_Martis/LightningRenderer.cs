using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Lightning : MonoBehaviour
{
    public Transform target;
    public float displacementAmount = 2.0f;
    public int segments = 15;

    private LineRenderer lineRenderer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            
        }
    }

    public void GenerateLightning(Vector3 start, Vector3 end)
    {
        List<Vector3> pointList = new List<Vector3>();
        pointList.Add(start);

        for (int i = 1; i < segments; i++)
        {
            float percentage = (float)i / (float)segments;
            Vector3 basePos = Vector3.Lerp(start, end, percentage);

            float offset = Mathf.PerlinNoise(i * 0.5f, Time.time * 5.0f) * 2.0f - 1.0f;
            Vector3 randomOffset = new Vector3(offset * displacementAmount, offset * displacementAmount, 0);

            pointList.Add(basePos + randomOffset);
        }
        pointList.Add(end);

        lineRenderer.positionCount = pointList.Count;
        lineRenderer.SetPositions(pointList.ToArray());
    }
}
