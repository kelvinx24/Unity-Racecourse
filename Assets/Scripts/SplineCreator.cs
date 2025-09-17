using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/**
 * Creates a spline or looping spline using Catmull Rom
 */

public class SplineCreator : MonoBehaviour
{
    public Transform[] points;

    public GameObject linePrefab;

    [Range(1, 100)]
    public float stepSize;

    public bool looping = false;

    // Start is called before the first frame update
    void Start()
    {
        if (looping)
        {
            DrawLoopingSpline();
        }
        else
        {
            DrawSpline();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Generates a non-looping spline
    public void DrawSpline()
    {
        // Start and end control points require additional neighbor so we create a mirrored point
        Vector3 startingPoint = points[0].position;
        Vector3 startingPointNeighbor = points[1].position;
        Vector3 mirroredStartingNeighbor = MirrorPoint(startingPoint, startingPointNeighbor);
        
        // This is the vector it needs and is incorporated in formula later on. Kept for intuition.
        Vector3 startingPointVelocity = startingPointNeighbor - mirroredStartingNeighbor;

        Vector3 endingPoint = points[points.Length - 1].position;
        Vector3 endingPointNeighbor = points[points.Length - 2].position;
        Vector3 mirroredEndingNeighbor = MirrorPoint(endingPoint, endingPointNeighbor);

        Vector3 endingPointVelocity = mirroredEndingNeighbor - endingPointNeighbor;

        // Create positions vector which included mirrored points
        Vector3[] positions = new Vector3[points.Length + 2];
        positions[0] = mirroredStartingNeighbor;
        positions[positions.Length - 1] = mirroredEndingNeighbor;
        for (int i = 0; i < points.Length; i++)
        {
            positions[i + 1] = points[i].position;
        }

        // Generate points on the curve between the current start and end
        // Starts from non-mirrored point and ends when the last non-mirrored point is the end point
        for (int j = 1; j < positions.Length - 2; j++)
        {
            Vector3 prevNeighbor = positions[j - 1];
            Vector3 start = positions[j];
            Vector3 end = positions[j + 1];
            Vector3 endNeighbor = positions[j + 2];

            for (float t = 0; t < 100; t += stepSize)
            {
                Vector3 curvePoint = CatmullRomPoint(prevNeighbor, start, end, endNeighbor, t / 100);
                GameObject curveObject = Instantiate(linePrefab);
                curveObject.transform.position = curvePoint;

            }
        }


        
    }

    // Generates a looping spline
    public void DrawLoopingSpline()
    {
        // Get all control point locations
        int numPoints = points.Length;
        Vector3[] positions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            positions[i] = points[i].position;
        }

        // Generate points on the curve between the current start and end
        // Offset by 1 to prevent negative results from modulus
        for (int j = 1; j < numPoints + 1; j++)
        {
            Vector3 prevNeighbor = positions[(j - 1) % numPoints];
            Vector3 start = positions[j % numPoints];
            Vector3 end = positions[(j + 1) % numPoints];
            Vector3 endNeighbor = positions[(j + 2) % numPoints];

            // The generation of said point on curve
            for (float t = 0; t < 100; t += stepSize)
            {
                Vector3 curvePoint = CatmullRomPoint(prevNeighbor, start, end, endNeighbor, t / 100);
                GameObject curveObject = Instantiate(linePrefab);
                curveObject.transform.position = curvePoint;

            }
        }
    }

    // Generate a point on a curve using Catmull-Rom based on what t is currently.
    // Connects p0 and p1 with a curve based on its neighbors (pPrev and pNext)
    public Vector3 CatmullRomPoint(Vector3 pPrev, Vector3 p0, Vector3 p1, Vector3 pNext, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        double x = 0.5 * (2 * p0[0] +
                        (-pPrev[0] + p1[0]) * t +
                        (2 * pPrev[0] - 5 * p0[0] + 4 * p1[0] - pNext[0]) * t2 +
                        (-pPrev[0] + 3 * p0[0] - 3 * p1[0] + pNext[0]) * t3);

        double y = 0.5 * (2 * p0[2] +
                        (-pPrev[2] + p1[2]) * t +
                        (2 * pPrev[2] - 5 * p0[2] + 4 * p1[2] - pNext[2]) * t2 +
                        (-pPrev[2] + 3 * p0[2] - 3 * p1[2] + pNext[2]) * t3);

        return new Vector3((float)x, 0, (float)y);

    }

    // Generates the mirrored position of a neighboring point across a point
    private Vector3 MirrorPoint(Vector3 endpoint, Vector3 neighbor)
    {
        Vector3 magnitude = neighbor - endpoint;
        Vector3 mirrored = endpoint + -magnitude;
        return mirrored;
    }
}
