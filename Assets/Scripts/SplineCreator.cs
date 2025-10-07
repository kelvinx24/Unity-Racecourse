using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Creates a spline or looping spline using Catmull Rom
 */

public class SplineCreator
{ 
    public static SplinePath CreateSplinePath(List<Vector3> controlPointPositions, int samplesPerSegment)
    {
        int numPoints = controlPointPositions.Count;
        float cumulative = 0;
        List<SegmentSample> samples = new List<SegmentSample>();
        List<SegmentSample> controlPointSamples = new List<SegmentSample>();

        for (int i = 0; i < numPoints; i++)
        {
            int prevNeighborIndex = i == 0 ? numPoints - 1 : i - 1 % numPoints;
            Vector3 prevNeighbor = controlPointPositions[prevNeighborIndex];
            Vector3 start = controlPointPositions[i % numPoints];
            Vector3 end = controlPointPositions[(i + 1) % numPoints];
            Vector3 endNeighbor = controlPointPositions[(i + 2) % numPoints];

            Vector3 previousPoint = CatmullRomPoint(prevNeighbor, start, end, endNeighbor, 0f);

            // The generation of said point on curve

            // Starting from 1 as we do not want to duplicate at control points
            // (first control point is covered by end of last segment)
            for (float j = 1; j < samplesPerSegment + 1; j++)
            {
                float segT = j / samplesPerSegment;
                Vector3 curvePoint = CatmullRomPoint(prevNeighbor, start, end, endNeighbor, segT);
                Vector3 tangent = CatmullRomTangent(prevNeighbor, start, end, endNeighbor, segT).normalized;
                Vector3 normal = Vector3.Cross(Vector3.up, tangent).normalized;

                // Calculate the total arc length of the segment
                float distanceX = curvePoint.x - previousPoint.x;
                float distanceY = curvePoint.z - previousPoint.z;
                float arcLength = Mathf.Sqrt(distanceY * distanceY + distanceX * distanceX);
                cumulative += arcLength;

                // Cache sample information in table to be accessed later  
                // to map distance traveled to segment and segment progress (t)
                SegmentSample sample = new SegmentSample(cumulative, samples.Count, i, segT, curvePoint, tangent, normal);
                samples.Add(sample);
                //Debug.Log(segmentSamples[sampleIndex]); 

                previousPoint = curvePoint;

                if (segT == 1f)
                {
                    controlPointSamples.Add(sample);
                }
            }
        }

        return new SplinePath(controlPointPositions, controlPointSamples, samples, cumulative);
    }

    public static SplinePath CreateNewOffsetSplineByNormal(SplinePath current, float factor, int samplesPerSegment)
    {
        List<Vector3> outerControlPoints = new List<Vector3>();
        List<SegmentSample> controlPointSamples = current.GetControlPointSamples();

        for (int i = 0; i < controlPointSamples.Count; i++)
        {
            Vector3 pointNormal = controlPointSamples[i].Normal;
            Vector3 offsetPosition = controlPointSamples[i].Position + pointNormal * factor;
            outerControlPoints.Add(offsetPosition);
        }

        return CreateSplinePath(outerControlPoints, samplesPerSegment);
    }

    // Generate a point on a curve using Catmull-Rom based on what t is currently.
    // Connects p0 and p1 with a curve based on its neighbors (pPrev and pNext)
    public static Vector3 CatmullRomPoint(Vector3 pPrev, Vector3 p0, Vector3 p1, Vector3 pNext, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 point = 0.5f * (2 * p0 +
                        (-pPrev + p1) * t +
                        (2 * pPrev - 5 * p0 + 4 * p1 - pNext) * t2 +
                        (-pPrev + 3 * p0 - 3 * p1 + pNext) * t3);


        return point;

    }

    // Calculates the derivative of Catmull-Rom spline at t (for tangent / forward)
    public static Vector3 CatmullRomTangent(Vector3 pPrev, Vector3 p0, Vector3 p1, Vector3 pNext, float t)
    {
        float t2 = t * t;

        return 0.5f * (
            (-pPrev + p1) +
            (4f * pPrev - 10f * p0 + 8f * p1 - 2f * pNext) * t +
            (-3f * pPrev + 9f * p0 - 9f * p1 + 3f * pNext) * t2
        );

    }


    public static Vector3 CatmullRomSecondDerivative(Vector3 pPrev, Vector3 p0, Vector3 p1, Vector3 pNext, float t)
    {
        return 0.5f * (
            (4f * pPrev - 10f * p0 + 8f * p1 - 2f * pNext) +
            (-6f * pPrev + 18f * p0 - 18f * p1 + 6f * pNext) * t
        );
    }

    public static float Curvature(Vector3 pPrev, Vector3 p0, Vector3 p1, Vector3 pNext, float t)
    {
        Vector3 d1 = CatmullRomTangent(pPrev, p0, p1, pNext, t);
        Vector2 d2 = CatmullRomSecondDerivative(pPrev, p0, p1, pNext, t);

        float crossMag = Vector3.Cross(d1, d2).magnitude;
        float denominator = Mathf.Pow(d1.magnitude, 3);

        if (denominator < 1e-6f) return 0f;
        return crossMag / denominator;
    }

    // Generates the mirrored position of a neighboring point across a point
    private static Vector3 MirrorPoint(Vector3 endpoint, Vector3 neighbor)
    {
        Vector3 magnitude = neighbor - endpoint;
        Vector3 mirrored = endpoint + -magnitude;
        return mirrored;
    }
}
