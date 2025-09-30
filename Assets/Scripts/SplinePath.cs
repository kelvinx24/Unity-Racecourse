using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SplinePath
{
    public List<Vector3> controlPoints = new List<Vector3>();

    private List<SegmentSample> controlPointSamples = new List<SegmentSample>();

    private List<SegmentSample> samplesTable = new List<SegmentSample>();

    private float cumulativeLength;

    public SplinePath(List<Vector3> controlPoints, List<SegmentSample> controlPointSamples, List<SegmentSample> samplesTable, float cumulativeLength)
    {
        this.controlPoints = controlPoints;
        this.controlPointSamples = controlPointSamples;
        this.samplesTable = samplesTable;
        this.cumulativeLength = cumulativeLength;
    }

    public List<SegmentSample> GetControlPointSamples()
    {
        return controlPointSamples;
    }

    public List<SegmentSample> GetSamplesTable() { return samplesTable; }

    // Returns the closest point position on the sampled spline
    public Vector3 ClosestPointPosition(Vector3 target, Vector3 targetDirection)
    {
        int bestIndex = FindClosestSampleIndex(target, targetDirection);
        //return samplesTable[bestIndex].Position;
        return RefineClosestPosition(target, bestIndex);
    }

    // Returns the tangent at the closest point
    public Vector3 ClosestPointTangent(Vector3 target, Vector3 targetDirection)
    {
        int bestIndex = FindClosestSampleIndex(target, targetDirection);

        // For better accuracy, also project onto segment and interpolate tangents
        return RefineClosestTangent(target, bestIndex);
        //return samplesTable[bestIndex].Tangent;
    }

    // Find the index of the closest sample
    private int FindClosestSampleIndex(Vector3 target, Vector3 targetDirection)
    {
        if (samplesTable == null || samplesTable.Count == 0)
            throw new System.ArgumentException("Samples list is empty");

        float bestDist = float.MaxValue;
        int bestIndex = 0;

        for (int i = 0; i < samplesTable.Count; i++)
        {
            Vector3 candidate = samplesTable[i].Position;
            Vector3 diff = candidate - target;
            if (Vector3.Dot(diff, targetDirection) < 0f)
            {
                continue;
            }

            float dist = diff.sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    private Vector3 RefineClosestPosition(Vector3 target, int bestIndex)
    {
        Vector3 bestPoint = samplesTable[bestIndex].Position;

        if (bestIndex > 0)
            bestPoint = ClosestPointOnSegment(samplesTable[bestIndex - 1].Position, samplesTable[bestIndex].Position, target, bestPoint);

        if (bestIndex < samplesTable.Count - 1)
            bestPoint = ClosestPointOnSegment(samplesTable[bestIndex].Position, samplesTable[bestIndex + 1].Position, target, bestPoint);

        return bestPoint;
    }

    // Refine tangent using neighbor segment projection & interpolation
    private Vector3 RefineClosestTangent(Vector3 target, int bestIndex)
    {
        Vector3 bestPos = samplesTable[bestIndex].Position;
        Vector3 bestTan = samplesTable[bestIndex].Tangent;

        if (bestIndex > 0)
        {
            (Vector3 proj, float t) = ClosestPointOnSegmentWithParam(samplesTable[bestIndex - 1].Position, samplesTable[bestIndex].Position, target);
            if ((proj - target).sqrMagnitude < (bestPos - target).sqrMagnitude)
            {
                bestPos = proj;
                bestTan = Vector3.Lerp(samplesTable[bestIndex - 1].Tangent, samplesTable[bestIndex].Tangent, t).normalized;
            }
        }

        if (bestIndex < samplesTable.Count - 1)
        {
            (Vector3 proj, float t) = ClosestPointOnSegmentWithParam(samplesTable[bestIndex].Position, samplesTable[bestIndex + 1].Position, target);
            if ((proj - target).sqrMagnitude < (bestPos - target).sqrMagnitude)
            {
                bestPos = proj;
                bestTan = Vector3.Lerp(samplesTable[bestIndex].Tangent, samplesTable[bestIndex + 1].Tangent, t).normalized;
            }
        }

        return bestTan.normalized;
    }

    // Project onto segment, return point
    private static Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p, Vector3 currentBest)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);

        Vector3 proj = a + t * ab;
        if ((proj - p).sqrMagnitude < (currentBest - p).sqrMagnitude)
            return proj;

        return currentBest;
    }

    // Project onto segment, return both point and parameter t
    private static (Vector3, float) ClosestPointOnSegmentWithParam(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);

        return (a + t * ab, t);
    }
}
