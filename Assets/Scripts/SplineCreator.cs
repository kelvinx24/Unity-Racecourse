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

        // Binary search for closest two samples
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

        // Covers start of the track
        SegmentSample firstSample = new SegmentSample(0f, 0, 0f, segmentSamples[segmentSamples.Length - 1].Position);
        if (low > 0)
        {
            firstSample = segmentSamples[low - 1];
        }

        SegmentSample secondSample = segmentSamples[low];

        Debug.Log("Cumulative Distance: " + currentDistance);
        Debug.Log("First End: " + firstSample);
        Debug.Log("Second End: " + secondSample);

        // Calculates progress between the two samples to get t for current segment
        float denominator = secondSample.Cumulative - firstSample.Cumulative;
        float scaler = denominator == 0 ? 0f : (currentDistance - firstSample.Cumulative) / denominator;
        scaler = Mathf.Clamp01(scaler);
        // Since we do not include start of segments, we convert the end of the previous segment if necessary
        float firstT = firstSample.SegmentT % 1;
        float interpolatedT = firstT + scaler * (secondSample.SegmentT - firstT);

        int numPoints = positions.Length;

        // Using t and the current curve's points, we determine the position, tangent, and normal
        // First and second should always be in the same curve; however
        // second segment index is used for same reason as last comment
        int prevIndex = Math.Abs((secondSample.SegmentIndex - 1) % numPoints);
        Vector3 prev = positions[prevIndex];
        Vector3 first = positions[(secondSample.SegmentIndex) % numPoints];
        Vector3 second = positions[(secondSample.SegmentIndex + 1) % numPoints];
        Vector3 next = positions[(secondSample.SegmentIndex + 2) % numPoints];

        Vector3 runnerPos = CatmullRomPoint(prev, first, second, next, interpolatedT);
        Debug.Log("Prev: " + prev);
        Debug.Log("First: " + first);
        Debug.Log("Second: " + second);
        Debug.Log("Next: " + next);
        Debug.Log("T: " + interpolatedT);
        Debug.Log("Final Position: " + runnerPos);

        Vector3 tangent = CatmullRomTangent(prev, first, second, next, interpolatedT).normalized;
        Quaternion heading = Quaternion.LookRotation(tangent, Vector3.up);

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


        // Generate sample points on the curve between the current start and end
        for (int j = 0; j < numPoints; j++)
        {
            // Negative mods returns negative in C#
            int prevNeighborIndex = Math.Abs((j - 1) % numPoints);

            Vector3 prevNeighbor = positions[prevNeighborIndex];
            Vector3 start = positions[j % numPoints];
            Vector3 end = positions[(j + 1) % numPoints];
            Vector3 endNeighbor = positions[(j + 2) % numPoints];

            Vector3 previousPoint = CatmullRomPoint(prevNeighbor, start, end, endNeighbor, 0f);

            float segmentArc = 0;
            // The generation of said point on curve
            // Starting from 1 as we do not want to duplicate at control points
            // (first control point is covered by end of last segment)
            for (float k = 1; k < samplesPerSegment + 1; k++)
            {
                float segT = k / samplesPerSegment;
                Vector3 curvePoint = CatmullRomPoint(prevNeighbor, start, end, endNeighbor, segT);

                GameObject curveObject = Instantiate(linePrefab);
                curveObject.transform.position = curvePoint;

                // Calculate the total arc length of the segment
                float distanceX = curvePoint.x - previousPoint.x;
                float distanceY = curvePoint.z - previousPoint.z;
                float arcLength = Mathf.Sqrt(distanceY * distanceY + distanceX * distanceX);
                cumulativeArcLength += arcLength;
                segmentArc += arcLength;

                // Cache sample information in table to be accessed later  
                // to map distance traveled to segment and segment progress (t)
                int sampleIndex = j * samplesPerSegment + ((int)k - 1);
                segmentSamples[sampleIndex] = new(cumulativeArcLength, j, segT, curvePoint);
                Debug.Log(segmentSamples[sampleIndex]); 

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

    // Calculates the derivative of Catmull-Rom spline at t (for tangent / forward)
    public Vector3 CatmullRomTangent(Vector3 pPrev, Vector3 p0, Vector3 p1, Vector3 pNext, float t)
    {
        float t2 = t * t;

        return 0.5f * (
            (-pPrev + p1) +
            (4f * pPrev - 10f * p0 + 8f * p1 - 2f * pNext) * t +
            (-3f * pPrev + 9f * p0 - 9f * p1 + 3f * pNext) * t2
        );

    }

    // Generates the mirrored position of a neighboring point across a point
    private Vector3 MirrorPoint(Vector3 endpoint, Vector3 neighbor)
    {
        Vector3 magnitude = neighbor - endpoint;
        Vector3 mirrored = endpoint + -magnitude;
        return mirrored;
    }
}
