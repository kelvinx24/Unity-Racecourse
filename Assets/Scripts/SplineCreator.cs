using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UIElements;

/**
 * Creates a spline or looping spline using Catmull Rom
 */

public class SplineCreator : MonoBehaviour
{
    private record SegmentSample(float Cumulative, int SegmentIndex, float SegmentT, Vector3 Position);

    public Transform[] points;

    public GameObject linePrefab;

    [Range(1, 200)]
    public int samplesPerSegment;

    public bool looping = false;

    private float cumulativeArcLength = 0;

    private Vector3[] positions;

    private SegmentSample[] segmentSamples;



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

        Debug.Log("Arc Length: " + cumulativeArcLength);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Get the racer's next position on the track based on its speed and current position
    public RacerStatus AdvanceRacer(float alreadyCovered, float speed, float deltaTime)
    {
        float currentDistance = (alreadyCovered + speed * deltaTime) % cumulativeArcLength;
        Debug.Log("Starting Distance: " +  alreadyCovered);
        Debug.Log("Current Distance: " + currentDistance);

        /*
        if (currentDistance < segmentSamples[0].Cumulative)
        {
            SegmentSample first = segmentSamples[segmentSamples.Length - 1];
            SegmentSample second = segmentSamples[0];

            float denominator = second.Cumulative - 0f;
            float scaler = denominator == 0 ? 0f : (currentDistance - 0f) / denominator;
            scaler = Mathf.Clamp01(scaler);

            float interpolatedT = 0f + scaler * (second.SegmentT - 0f);

        }
        */

        int low = 0, high = segmentSamples.Length - 1;
        while (low < high)
        {
            int mid = (low + high) / 2;
            if (segmentSamples[mid].Cumulative < currentDistance)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }

        }

        SegmentSample firstSample = new SegmentSample(0f, 0, 0f, segmentSamples[segmentSamples.Length - 1].Position);

        if (low > 0)
        {
            firstSample = segmentSamples[low - 1];
        }

        SegmentSample secondSample = segmentSamples[low];

        float denominator = secondSample.Cumulative - firstSample.Cumulative;
        float scaler = denominator == 0 ? 0f : (currentDistance - firstSample.Cumulative) / denominator;
        scaler = Mathf.Clamp01(scaler);

        float interpolatedT = firstSample.SegmentT + scaler * (secondSample.SegmentT - firstSample.SegmentT);

        int numPoints = positions.Length;

        int prevIndex = Math.Abs((secondSample.SegmentIndex - 1) % numPoints);
        Vector3 prev = positions[prevIndex];
        Vector3 first = positions[(secondSample.SegmentIndex) % numPoints];
        Vector3 second = positions[(secondSample.SegmentIndex + 1) % numPoints];
        Vector3 next = positions[(secondSample.SegmentIndex + 2) % numPoints];

        Vector3 runnerPos = CatmullRomPoint(prev, first, second, next, interpolatedT);
        Vector3 tangent = CatmullRomTangent(prev, first, second, next, interpolatedT);
        Vector3 heading = Vector3.Cross(Vector3.up, tangent);

        return new RacerStatus(currentDistance, runnerPos, heading);
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

            for (float t = 0; t < 100; t += samplesPerSegment)
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
        segmentSamples = new SegmentSample[points.Length * samplesPerSegment];

        // Get all control point locations
        int numPoints = points.Length;
        positions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            positions[i] = points[i].position;
        }


        // Generate points on the curve between the current start and end
        // Offset by 1 to prevent negative results from modulus
        for (int j = 0; j < numPoints; j++)
        {
            int prevNeighborIndex = Math.Abs((j - 1) % numPoints);

            Vector3 prevNeighbor = positions[prevNeighborIndex];
            Vector3 start = positions[j % numPoints];
            Vector3 end = positions[(j + 1) % numPoints];
            Vector3 endNeighbor = positions[(j + 2) % numPoints];

            Vector3 previousPoint = CatmullRomPoint(prevNeighbor, start, end, endNeighbor, 0f);

            float segmentArc = 0;
            // The generation of said point on curve
            for (float k = 1; k < samplesPerSegment + 1; k++)
            {
                float segT = k / samplesPerSegment;
                Vector3 curvePoint = CatmullRomPoint(prevNeighbor, start, end, endNeighbor, segT);

                GameObject curveObject = Instantiate(linePrefab);
                curveObject.transform.position = curvePoint;

                float distanceX = curvePoint.x - previousPoint.x;
                float distanceY = curvePoint.z - previousPoint.z;
                float arcLength = Mathf.Sqrt(distanceY * distanceY + distanceX * distanceX);
                cumulativeArcLength += arcLength;
                segmentArc += arcLength;

                int sampleIndex = j * samplesPerSegment + ((int)k - 1);
                segmentSamples[sampleIndex] = new(cumulativeArcLength, j, segT, curvePoint);

                previousPoint = curvePoint;
            }

            Debug.Log("Segment Arc Length: " + segmentArc);
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

    public Vector3 CatmullRomTangent(Vector3 pPrev, Vector3 p0, Vector3 p1, Vector3 pNext, float t)
    {
        float t2 = t * t;

        Vector3 tangent = (6 * t2 - 6 * t) * p0
                        + (3 * t2 - 4 * t + 1) * pPrev
                        + (-6 * t2 + 6 * t) * p1
                        + (3 * t2 - 2 * t) * pNext;

        return tangent;

    }

    // Generates the mirrored position of a neighboring point across a point
    private Vector3 MirrorPoint(Vector3 endpoint, Vector3 neighbor)
    {
        Vector3 magnitude = neighbor - endpoint;
        Vector3 mirrored = endpoint + -magnitude;
        return mirrored;
    }
}
